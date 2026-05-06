using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Catalogos;
using Backend.Domain.Entities.Equipos;
using Backend.Domain.Entities.Proyectos;
using Backend.Domain.Entities.Tareas;
using Backend.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder de datos demo realistas para CDT (estilo Jira/ClickUp).
/// Idempotente: si detecta que ya existen >= 50 usuarios, se considera seedeado y no hace nada.
/// Crea: 2 admins fijos + 60 usuarios distribuidos en estructura jerárquica de equipos +
/// 7 proyectos con claves Jira-style + listas (columnas Kanban) + ~150 tareas con estados/prioridades/responsables.
/// </summary>
public static class DemoSeeder
{
    // Pool de nombres y apellidos colombianos comunes (determinístico).
    private static readonly string[] Nombres =
    {
        "Andrés", "María", "Juan", "Laura", "Carlos", "Sofía", "Diego", "Valentina", "Luis", "Camila",
        "Sebastián", "Daniela", "Felipe", "Isabella", "Santiago", "Mariana", "Mateo", "Salomé", "David", "Paula",
        "Nicolás", "Catalina", "Alejandro", "Manuela", "Ricardo", "Gabriela", "Tomás", "Juliana", "Esteban", "Ana",
        "Cristian", "Carolina", "Javier", "Natalia", "Manuel", "Lina", "Pablo", "Andrea", "Hernán", "Mónica",
        "Iván", "Diana", "Óscar", "Tatiana", "Mauricio", "Liliana", "Fernando", "Sandra", "Jaime", "Adriana",
        "Rodrigo", "Ximena", "Álvaro", "Beatriz", "Germán", "Carmen", "Ramiro", "Patricia", "Hugo", "Marcela"
    };

    private static readonly string[] Apellidos =
    {
        "García", "Rodríguez", "Martínez", "López", "González", "Hernández", "Pérez", "Sánchez", "Ramírez",
        "Torres", "Flores", "Rivera", "Gómez", "Díaz", "Reyes", "Cruz", "Morales", "Ortiz", "Gutiérrez",
        "Chávez", "Ramos", "Ruiz", "Álvarez", "Mendoza", "Vargas", "Castillo", "Jiménez", "Romero", "Herrera",
        "Medina", "Aguilar", "Vega", "Castro", "Contreras", "Espinoza", "Fernández", "Suárez", "Cárdenas",
        "Quintero", "Restrepo", "Bedoya", "Arango", "Ospina", "Marín", "Cardona", "Gallego", "Henao", "Mejía"
    };

    public static async Task SeedAsync(CdtDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        // Idempotencia: si ya hay 50+ usuarios, asumimos que el demo ya fue seedeado.
        var totalUsuarios = await db.Usuarios.CountAsync(ct);
        if (totalUsuarios >= 50)
        {
            logger?.LogInformation("DemoSeeder: BD ya tiene {Total} usuarios. Saltando seed.", totalUsuarios);
            return;
        }

        logger?.LogInformation("DemoSeeder: iniciando seed de datos demo (60 usuarios + equipos + proyectos + tareas)...");

        var rnd = new Random(42); // determinístico

        // Catálogos ya están poblados por CatalogosSeeder antes de llegar aquí.
        var estados = await db.Estados.OrderBy(x => x.Orden).ToListAsync(ct);
        var prioridades = await db.Prioridades.OrderBy(x => x.Orden).ToListAsync(ct);
        var tipos = await db.TiposActividad.OrderBy(x => x.Orden).ToListAsync(ct);

        // ============== USUARIOS ==============
        // 2 admins fijos
        var passwordDemoHash = BCrypt.Net.BCrypt.HashPassword("Demo1234*");

        var admin1 = new UsuarioEntity
        {
            Nombre = "Demo Admin",
            Correo = "demo@orf.local",
            ClaveHash = passwordDemoHash,
            RolGlobal = "Admin",
            Estado = true,
            FechaConfirmacionEmail = DateTime.UtcNow.AddDays(-30),
        };
        var admin2 = new UsuarioEntity
        {
            Nombre = "Roberto Ramírez",
            Correo = "rmramirez@orf.com.co",
            ClaveHash = passwordDemoHash,
            RolGlobal = "Admin",
            Estado = true,
            FechaConfirmacionEmail = DateTime.UtcNow.AddDays(-30),
        };
        db.Usuarios.AddRange(admin1, admin2);

        // 60 usuarios demo: 8 líderes + 52 miembros
        var demoUsers = new List<UsuarioEntity>();
        var correosUsados = new HashSet<string> { admin1.Correo, admin2.Correo };

        for (int i = 0; i < 60; i++)
        {
            string nombre, apellido, correo;
            int intentos = 0;
            do
            {
                nombre = Nombres[rnd.Next(Nombres.Length)];
                apellido = Apellidos[rnd.Next(Apellidos.Length)];
                var slug = $"{Slugify(nombre)}.{Slugify(apellido)}";
                correo = $"{slug}{(intentos > 0 ? intentos.ToString() : "")}@orf.com.co";
                intentos++;
            } while (correosUsados.Contains(correo));
            correosUsados.Add(correo);

            // Primeros 8 son líderes, resto miembros
            var rol = i < 8 ? "Lider" : "Miembro";

            demoUsers.Add(new UsuarioEntity
            {
                Nombre = $"{nombre} {apellido}",
                Correo = correo,
                ClaveHash = passwordDemoHash,
                RolGlobal = rol,
                Estado = true,
                FechaConfirmacionEmail = DateTime.UtcNow.AddDays(-rnd.Next(1, 60)),
            });
        }
        db.Usuarios.AddRange(demoUsers);
        await db.SaveChangesAsync(ct); // persistimos para tener IDs

        var lideres = demoUsers.Take(8).ToList();
        var miembrosBase = demoUsers.Skip(8).ToList(); // 52 miembros para repartir

        // ============== EQUIPOS JERÁRQUICOS ==============
        // Estructura:
        // ORF (raíz)
        // ├── TI
        // │   ├── Desarrollo
        // │   ├── Soporte
        // │   └── Infraestructura
        // ├── Comercial
        // │   ├── Ventas
        // │   └── Marketing
        // ├── Operaciones
        // │   ├── Logística
        // │   └── Despachos
        // └── Administración
        //     ├── Contabilidad
        //     └── Recursos Humanos

        var orf = new EquipoEntity { Nombre = "ORF", IdLider = admin2.Id };
        db.Equipos.Add(orf);
        await db.SaveChangesAsync(ct);

        EquipoEntity Hijo(string nombre, int? idPadre, int idLider) => new()
        {
            Nombre = nombre,
            IdEquipoPadre = idPadre,
            IdLider = idLider,
        };

        var ti = Hijo("TI", orf.Id, lideres[0].Id);
        var comercial = Hijo("Comercial", orf.Id, lideres[1].Id);
        var operaciones = Hijo("Operaciones", orf.Id, lideres[2].Id);
        var admon = Hijo("Administración", orf.Id, lideres[3].Id);
        db.Equipos.AddRange(ti, comercial, operaciones, admon);
        await db.SaveChangesAsync(ct);

        var desarrollo = Hijo("Desarrollo", ti.Id, lideres[0].Id);
        var soporte = Hijo("Soporte", ti.Id, lideres[4].Id);
        var infra = Hijo("Infraestructura", ti.Id, lideres[4].Id);
        var ventas = Hijo("Ventas", comercial.Id, lideres[1].Id);
        var marketing = Hijo("Marketing", comercial.Id, lideres[5].Id);
        var logistica = Hijo("Logística", operaciones.Id, lideres[2].Id);
        var despachos = Hijo("Despachos", operaciones.Id, lideres[6].Id);
        var contabilidad = Hijo("Contabilidad", admon.Id, lideres[3].Id);
        var rrhh = Hijo("Recursos Humanos", admon.Id, lideres[7].Id);
        db.Equipos.AddRange(desarrollo, soporte, infra, ventas, marketing, logistica, despachos, contabilidad, rrhh);
        await db.SaveChangesAsync(ct);

        // Distribuir miembros entre los 9 equipos hoja (round-robin con jitter)
        var equiposHoja = new[] { desarrollo, soporte, infra, ventas, marketing, logistica, despachos, contabilidad, rrhh };
        for (int i = 0; i < miembrosBase.Count; i++)
        {
            var equipo = equiposHoja[i % equiposHoja.Length];
            db.EquiposMiembros.Add(new EquipoMiembroEntity
            {
                IdEquipo = equipo.Id,
                IdUsuario = miembrosBase[i].Id,
                FechaAgregado = DateTime.UtcNow.AddDays(-rnd.Next(1, 90)),
            });
        }
        // Líderes: agregar como miembros de su propio equipo (les pertenecen)
        foreach (var lider in lideres)
        {
            // El líder pertenece a UN equipo aleatorio donde tenga sentido (reuso del pool de hoja)
            var equipo = equiposHoja[rnd.Next(equiposHoja.Length)];
            var yaMiembro = await db.EquiposMiembros
                .AnyAsync(m => m.IdEquipo == equipo.Id && m.IdUsuario == lider.Id, ct);
            if (!yaMiembro)
            {
                db.EquiposMiembros.Add(new EquipoMiembroEntity
                {
                    IdEquipo = equipo.Id,
                    IdUsuario = lider.Id,
                    FechaAgregado = DateTime.UtcNow.AddDays(-rnd.Next(1, 90)),
                });
            }
        }
        await db.SaveChangesAsync(ct);

        // ============== PROYECTOS ==============
        var proyectosDef = new[]
        {
            (Clave: "ANI",  Nombre: "Aniro - Plataforma interna",       Equipo: desarrollo, Desc: "Desarrollo del nuevo portal interno de empleados."),
            (Clave: "TAR",  Nombre: "Tareas TDH",                        Equipo: desarrollo, Desc: "App de control de tareas (este mismo proyecto)."),
            (Clave: "SOP",  Nombre: "Soporte N1/N2",                     Equipo: soporte,    Desc: "Tickets de mesa de ayuda y atención a usuarios."),
            (Clave: "INFRA",Nombre: "Migración a Azure",                 Equipo: infra,      Desc: "Migración de servidores on-premise a Azure."),
            (Clave: "COM",  Nombre: "Campaña Q3 2026",                   Equipo: marketing,  Desc: "Lanzamiento de campaña comercial del tercer trimestre."),
            (Clave: "LOG",  Nombre: "Optimización de rutas",             Equipo: logistica,  Desc: "Análisis y optimización de rutas de despacho."),
            (Clave: "RRHH", Nombre: "Onboarding 2026",                   Equipo: rrhh,       Desc: "Proceso de inducción y capacitación nuevo personal."),
        };

        var proyectos = new List<ProyectoEntity>();
        foreach (var pd in proyectosDef)
        {
            var p = new ProyectoEntity
            {
                Clave = pd.Clave,
                Nombre = pd.Nombre,
                Descripcion = pd.Desc,
                IdEquipo = pd.Equipo.Id,
                IdCreador = admin2.Id,
                UltimoNumeroTarea = 0,
            };
            proyectos.Add(p);
            db.Proyectos.Add(p);
        }
        await db.SaveChangesAsync(ct);

        // ============== LISTAS (columnas Kanban por proyecto) ==============
        var listasPorProyecto = new Dictionary<int, List<ListaEntity>>();
        var nombresColumnas = new (string Nombre, string Color)[]
        {
            ("Por hacer",   "#94a3b8"),
            ("En progreso", "#3b82f6"),
            ("En revisión", "#a855f7"),
            ("Hecho",       "#22c55e"),
        };

        foreach (var p in proyectos)
        {
            var listas = new List<ListaEntity>();
            for (int i = 0; i < nombresColumnas.Length; i++)
            {
                var lista = new ListaEntity
                {
                    Nombre = nombresColumnas[i].Nombre,
                    Color = nombresColumnas[i].Color,
                    Orden = i,
                    IdProyecto = p.Id,
                    IdCreador = admin2.Id,
                };
                listas.Add(lista);
                db.Listas.Add(lista);
            }
            listasPorProyecto[p.Id] = listas;
        }
        await db.SaveChangesAsync(ct);

        // ============== TAREAS ==============
        var titulosPorTipo = new Dictionary<string, string[]>
        {
            ["Tarea"] = new[] {
                "Configurar repositorio Git", "Documentar endpoints REST", "Agregar tests unitarios al login",
                "Refactorizar servicio de notificaciones", "Migrar logs a Serilog", "Limpiar dependencias obsoletas",
                "Actualizar README", "Crear pipeline CI", "Agregar healthcheck endpoint", "Definir estándar de naming",
            },
            ["Historia"] = new[] {
                "Como usuario quiero recuperar mi contraseña", "Como admin quiero ver auditoría de cambios",
                "Como líder quiero asignar tareas a mi equipo", "Como usuario quiero filtrar mi backlog",
                "Como admin quiero exportar reportes a Excel", "Como usuario quiero notificaciones por email",
                "Como líder quiero ver carga de trabajo del equipo", "Como usuario quiero buscar por etiqueta",
            },
            ["Bug"] = new[] {
                "Login falla con email en mayúsculas", "Pantalla blanca al cerrar sesión",
                "Botón de eliminar no responde en Safari", "Token JWT no expira correctamente",
                "Error 500 al subir archivo grande", "Notificación se envía dos veces",
                "Listado se desordena al filtrar", "Caracteres especiales rompen búsqueda",
            },
            ["Épica"] = new[] {
                "Sistema completo de notificaciones", "Módulo de reportería avanzada",
                "Integración con AD corporativo", "Migración a microservicios",
            },
            ["Soporte"] = new[] {
                "Usuario reporta error de acceso", "Solicitud de cambio de contraseña",
                "Problema con impresora de oficina", "Configurar correo en celular nuevo",
                "Restaurar archivos eliminados", "Error en módulo de facturación",
                "Lentitud al abrir el sistema", "Bloqueo de cuenta por intentos fallidos",
            },
        };

        var todosUsuarios = await db.Usuarios.ToListAsync(ct);
        var ahora = DateTime.UtcNow;
        int totalTareasCreadas = 0;

        foreach (var p in proyectos)
        {
            var listas = listasPorProyecto[p.Id];
            int tareasParaEsteProyecto = rnd.Next(15, 26); // 15-25 tareas por proyecto
            int ordenEnLista0 = 0, ordenEnLista1 = 0, ordenEnLista2 = 0, ordenEnLista3 = 0;

            for (int i = 0; i < tareasParaEsteProyecto; i++)
            {
                var tipo = tipos[rnd.Next(tipos.Count)];
                var titulosArr = titulosPorTipo.GetValueOrDefault(tipo.Nombre, titulosPorTipo["Tarea"]);
                var titulo = titulosArr[rnd.Next(titulosArr.Length)];

                // Distribución realista: 40% Por hacer, 30% En progreso, 15% En revisión, 15% Hecho
                int idxLista;
                int dado = rnd.Next(100);
                if (dado < 40) idxLista = 0;
                else if (dado < 70) idxLista = 1;
                else if (dado < 85) idxLista = 2;
                else idxLista = 3;

                var lista = listas[idxLista];
                int orden = idxLista switch
                {
                    0 => ordenEnLista0++,
                    1 => ordenEnLista1++,
                    2 => ordenEnLista2++,
                    _ => ordenEnLista3++,
                };

                // Estado pareja con la columna (kanban-style)
                var estado = idxLista < estados.Count ? estados[idxLista] : estados[^1];

                p.UltimoNumeroTarea++;

                // Responsable: 75% un usuario aleatorio, 25% sin asignar
                int? idResp = rnd.Next(100) < 75 ? todosUsuarios[rnd.Next(todosUsuarios.Count)].Id : null;
                int? idInfo = todosUsuarios[rnd.Next(todosUsuarios.Count)].Id;

                // Fecha de vencimiento: 60% tienen, distribución pasado/futuro
                DateTime? venc = rnd.Next(100) < 60
                    ? ahora.AddDays(rnd.Next(-15, 45))
                    : null;

                var tarea = new TareaEntity
                {
                    Titulo = $"{titulo}",
                    Descripcion = idxLista == 3 ? "Completada como parte del avance del sprint." : null,
                    IdProyecto = p.Id,
                    IdLista = lista.Id,
                    IdTipoActividad = tipo.Id,
                    IdEstado = estado.Id,
                    IdPrioridad = prioridades[rnd.Next(prioridades.Count)].Id,
                    IdResponsable = idResp,
                    IdInformador = idInfo,
                    FechaVencimiento = venc,
                    NumeroEnProyecto = p.UltimoNumeroTarea,
                    Orden = orden,
                };
                db.Tareas.Add(tarea);
                totalTareasCreadas++;
            }
        }
        await db.SaveChangesAsync(ct);

        logger?.LogInformation(
            "DemoSeeder: completado. Usuarios={Usuarios}, Equipos={Equipos}, Proyectos={Proyectos}, Listas={Listas}, Tareas={Tareas}",
            await db.Usuarios.CountAsync(ct),
            await db.Equipos.CountAsync(ct),
            await db.Proyectos.CountAsync(ct),
            await db.Listas.CountAsync(ct),
            totalTareasCreadas);
    }

    private static string Slugify(string s)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in s.ToLowerInvariant())
        {
            sb.Append(c switch
            {
                'á' => 'a', 'é' => 'e', 'í' => 'i', 'ó' => 'o', 'ú' => 'u', 'ñ' => 'n',
                'ü' => 'u',
                _ => c
            });
        }
        return new string(sb.ToString().Where(char.IsLetterOrDigit).ToArray());
    }
}
