# CDT — Especificación V1

Sistema interno de **control de tareas** para **ORF**. Documento vivo y único contrato funcional de la V1.

## Contexto

- **Empresa**: ORF (single-tenant; todos los usuarios pertenecen a ORF).
- **Tamaño**: ~50 usuarios. Proyectos y tareas sin límite.
- **Inspiración funcional**: Jira / ClickUp recortado a lo esencial.
- **Stack**: ver [STACK_Y_CONVENCIONES.md](./STACK_Y_CONVENCIONES.md).
- **Hosting objetivo**: Windows + SQL Server (decisión final de despliegue queda para después).

## Alcance V1

### Sí incluido

- Auth: registro con confirmación de email, login, recuperación de contraseña por código de 6 dígitos, refresh token.
- Equipos jerárquicos (un equipo puede tener sub-equipos).
- Proyectos asignados a un equipo.
- Listas dentro de un proyecto (al menos una "Default" implícita).
- Tareas dentro de listas, con subtareas (`parent_id`).
- Vista Kanban **por lista** (columnas = estados).
- Asignación de tareas, prioridad, fechas (creación + límite), observaciones (log cronológico simple).
- Estados, prioridades y etiquetas **parametrizables** (admin gestiona el catálogo).
- Roles globales: `Admin` / `Líder` / `Miembro`.
- Invitación dual: admin/líder invita por email **o** admin asigna a usuario ya registrado.
- Notificaciones internas in-app (campana en topbar, polling 30 s).
- Email solo para: invite, confirmación de cuenta, reset de contraseña.
- Búsqueda de personas por nombre/correo dentro de ORF.

### Fuera de V1

- Adjuntos a tareas.
- Dependencias entre tareas.
- Sprints, milestones, roadmaps, Gantt.
- SLA, automations, custom fields.
- Multi-tenant.
- Realtime (SignalR/WebSockets) — V2.
- Integraciones externas (Slack, GitHub, etc.).

## Modelo de dominio

```
Usuario               (id, nombre, correo, claveHash, estado, fechaConfirmacionEmail, rolGlobal)
Equipo                (id, nombre, idEquipoPadre, idLider)
EquipoMiembro         (idEquipo, idUsuario)
Proyecto              (id, nombre, descripcion, idEquipo, idCreador, fechaCreacion)
Lista                 (id, idProyecto, nombre, orden)
Tarea                 (id, idLista, idTareaPadre, titulo, descripcion,
                       idEstado, idPrioridad, idAsignado, idCreador,
                       fechaCreacion, fechaLimite)
TareaObservacion      (id, idTarea, idAutor, texto, fecha)
TareaEtiqueta         (idTarea, idEtiqueta)             -- M:N
Etiqueta              (id, nombre, color)               -- catálogo
Estado                (id, nombre, color, orden)        -- catálogo
Prioridad             (id, nombre, color, orden)        -- catálogo
Notificacion          (id, idDestinatario, tipo, mensaje, idEntidadRelacionada,
                       fecha, fechaLeida)
Invitacion            (id, correo, idEquipoDestino, token,
                       fechaCreacion, fechaExpiracion, usada)
TokenResetPassword    (id, idUsuario, codigo6Digitos,
                       fechaCreacion, fechaExpiracion, usado)
RefreshToken          (id, idUsuario, token,
                       fechaCreacion, fechaExpiracion, revocado)
```

## Roles y permisos

| Rol | Alcance | Qué puede |
|---|---|---|
| **Admin** | Toda ORF | Todo: gestionar equipos, proyectos, usuarios, catálogos parametrizables. |
| **Líder** | Sus equipos + sub-equipos en cascada | Gestionar proyectos de sus equipos, crear/asignar tareas, ver miembros. |
| **Miembro** | Equipos donde pertenece | Ver y trabajar tareas en proyectos de sus equipos; crear tareas; observar; cambiar estado de tareas asignadas. |

Permisos derivados (no granulares). Validación dentro de cada use case del backend.

## Flujos clave

1. **Registro**: completa formulario → recibe email con código → confirma → cuenta activa **sin equipo** (espera asignación de admin).
2. **Login**: email + password → JWT (8 h) + refresh token (7 d).
3. **Reset password**: solicita reset → recibe código de 6 dígitos por correo → ingresa código + nueva password → cambio aplicado.
4. **Invitación**: admin/líder ingresa correo + equipo destino → email con link → usuario completa registro y queda en el equipo.
5. **Crear tarea**: usuario abre proyecto → lista → "Nueva tarea" → completa form → tarea creada con estado por defecto.
6. **Asignar tarea**: dropdown de miembros del proyecto → notificación interna al asignado.
7. **Kanban**: vista por lista, drag & drop entre columnas (estados).

## Vistas principales

| Vista | Quién la ve |
|---|---|
| Login / Registro / Reset password / Confirmar email | Público |
| Dashboard ("mis tareas") | Cualquier autenticado |
| Lista de proyectos (de sus equipos) | Líder + Miembro |
| Detalle de proyecto → listas → kanban | Miembros del equipo |
| Detalle de tarea (modal o página) | Miembros del equipo |
| Mis notificaciones | Cualquier autenticado |
| Admin: usuarios, equipos, catálogos, invitaciones | Solo Admin |

## Endpoints REST (resumen)

```
# Auth
POST   /api/auth/register
POST   /api/auth/confirm-email
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
POST   /api/auth/forgot-password
POST   /api/auth/reset-password

# Users
GET    /api/users/me
GET    /api/users           (búsqueda por nombre/correo)
PATCH  /api/users/me

# Equipos
GET    /api/equipos
POST   /api/equipos
PATCH  /api/equipos/{id}
DELETE /api/equipos/{id}
POST   /api/equipos/{id}/miembros
DELETE /api/equipos/{id}/miembros/{idUsuario}

# Proyectos
GET    /api/proyectos
POST   /api/proyectos
GET    /api/proyectos/{id}
PATCH  /api/proyectos/{id}
DELETE /api/proyectos/{id}

# Listas
GET    /api/proyectos/{idProyecto}/listas
POST   /api/proyectos/{idProyecto}/listas
PATCH  /api/listas/{id}
DELETE /api/listas/{id}

# Tareas
GET    /api/listas/{idLista}/tareas
POST   /api/listas/{idLista}/tareas
GET    /api/tareas/{id}
PATCH  /api/tareas/{id}
DELETE /api/tareas/{id}
POST   /api/tareas/{id}/observaciones

# Catálogos (admin)
GET/POST/PATCH/DELETE  /api/catalogos/estados
GET/POST/PATCH/DELETE  /api/catalogos/prioridades
GET/POST/PATCH/DELETE  /api/catalogos/etiquetas

# Invitaciones (admin / líder)
POST   /api/invitaciones
GET    /api/invitaciones
DELETE /api/invitaciones/{id}

# Notificaciones
GET    /api/notificaciones
POST   /api/notificaciones/{id}/leer
POST   /api/notificaciones/leer-todas
```

Detalle de payloads se documenta en cada módulo conforme se implementa.

## Plan de entrega

Orden de implementación (por dependencias):

1. **Fundaciones** — backend + frontend setup, andamiaje base copiado/adaptado de TDH.
2. **Auth completo** — registro + confirmación email + login + reset + refresh.
3. **Vista Admin básica** — login → perfil → ver catálogos sembrados.
4. **Equipos y Usuarios** — CRUD admin de usuarios y equipos jerárquicos.
5. **Proyectos** — CRUD.
6. **Listas + Tareas** — CRUD básico.
7. **Kanban** — vista con drag & drop por lista.
8. **Asignaciones + Notificaciones in-app**.
9. **Invitaciones por email + servicio de email**.

Lo que esté listo el **viernes 2026-05-08** se presenta como demo. El resto continúa en semanas posteriores.
