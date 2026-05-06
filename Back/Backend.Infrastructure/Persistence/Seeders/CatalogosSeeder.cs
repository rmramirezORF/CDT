using Backend.Domain.Entities.Catalogos;
using Backend.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder idempotente — corre en el startup. Si las tablas están vacías, las pobla con valores por defecto.
/// </summary>
public static class CatalogosSeeder
{
    public static async Task SeedAsync(CdtDbContext db, CancellationToken ct = default)
    {
        if (!await db.Estados.AnyAsync(ct))
        {
            db.Estados.AddRange(
                new EstadoEntity { Nombre = "Por hacer",   Color = "#94a3b8", Orden = 1 },
                new EstadoEntity { Nombre = "En curso",    Color = "#3b82f6", Orden = 2 },
                new EstadoEntity { Nombre = "En revisión", Color = "#a855f7", Orden = 3 },
                new EstadoEntity { Nombre = "Hecho",       Color = "#22c55e", Orden = 4 },
                new EstadoEntity { Nombre = "Cancelada",   Color = "#ef4444", Orden = 5 }
            );
        }

        if (!await db.Prioridades.AnyAsync(ct))
        {
            db.Prioridades.AddRange(
                new PrioridadEntity { Nombre = "Baja",    Color = "#94a3b8", Orden = 1 },
                new PrioridadEntity { Nombre = "Media",   Color = "#f59e0b", Orden = 2 },
                new PrioridadEntity { Nombre = "Alta",    Color = "#f97316", Orden = 3 },
                new PrioridadEntity { Nombre = "Urgente", Color = "#ef4444", Orden = 4 }
            );
        }

        if (!await db.Etiquetas.AnyAsync(ct))
        {
            db.Etiquetas.AddRange(
                new EtiquetaEntity { Nombre = "Bug",         Color = "#ef4444", Orden = 1 },
                new EtiquetaEntity { Nombre = "Feature",     Color = "#3b82f6", Orden = 2 },
                new EtiquetaEntity { Nombre = "Mejora",      Color = "#22c55e", Orden = 3 },
                new EtiquetaEntity { Nombre = "Documentación", Color = "#a855f7", Orden = 4 }
            );
        }

        if (!await db.TiposActividad.AnyAsync(ct))
        {
            db.TiposActividad.AddRange(
                new TipoActividadEntity { Nombre = "Tarea",    Color = "#3b82f6", Orden = 1, Icono = "circle-check" },
                new TipoActividadEntity { Nombre = "Historia", Color = "#22c55e", Orden = 2, Icono = "bookmark" },
                new TipoActividadEntity { Nombre = "Bug",      Color = "#ef4444", Orden = 3, Icono = "bug" },
                new TipoActividadEntity { Nombre = "Épica",    Color = "#a855f7", Orden = 4, Icono = "zap" },
                new TipoActividadEntity { Nombre = "Soporte",  Color = "#f59e0b", Orden = 5, Icono = "life-buoy" }
            );
        }

        // Dominios permitidos para registro: idempotente por dominio (no por tabla),
        // así si admin agrega/quita uno por UI, los defaults siguen presentes.
        string[] dominiosDefault = { "orf.com.co", "tdh.com.co" };
        foreach (var dominio in dominiosDefault)
        {
            var existe = await db.DominiosPermitidos.AnyAsync(d => d.Dominio == dominio, ct);
            if (!existe)
                db.DominiosPermitidos.Add(new DominioPermitidoEntity { Dominio = dominio });
        }

        await db.SaveChangesAsync(ct);
    }
}
