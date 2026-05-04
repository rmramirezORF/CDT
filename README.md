# CDT — Control de Tareas

Sistema interno de control de tareas para **ORF**. Inspirado funcionalmente en Jira / ClickUp, recortado a lo esencial.

## Stack

- **Backend**: .NET 9 + C# + Clean Architecture + EF Core 9 + SQL Server + JWT.
- **Frontend**: Vue 3 + TypeScript + Vite + Pinia + Vue Router + shadcn-vue + Tailwind 4.

Detalle completo en [`docs/STACK_Y_CONVENCIONES.md`](docs/STACK_Y_CONVENCIONES.md).

## Estructura del repositorio

```
CDT/
├── Back/      ← API .NET (Backend.Api / .Application / .Domain / .Infrastructure)
├── Front/     ← App Vue 3
├── docs/
│   ├── SPEC_V1.md                  ← contrato funcional V1
│   └── STACK_Y_CONVENCIONES.md     ← stack y convenciones
├── .agents/skills/                 ← skills para agentes IA
└── README.md
```

## Cómo correr (en construcción)

Las instrucciones se completarán en `Back/README.md` y `Front/README.md` a medida que se implemente cada parte.

## Documentación

- [Especificación V1](docs/SPEC_V1.md) — qué se construye y por qué.
- [Stack y convenciones](docs/STACK_Y_CONVENCIONES.md) — pila técnica, patrones, naming.
