# Stack y convenciones — CDT

Pila técnica, estructura y patrones obligatorios del proyecto. Si vas a escribir código en este repo, este es el documento que tenés que respetar.

## 1. Stack

### Backend

| Componente | Tecnología | Versión |
|---|---|---|
| Runtime | .NET | 9.0 |
| Lenguaje | C# (Nullable enable + ImplicitUsings enable) | 13 |
| Arquitectura | Clean Architecture (Api / Application / Domain / Infrastructure) | — |
| ORM | Entity Framework Core (SqlServer) | 9.x |
| Base de datos | SQL Server | 2022 |
| Auth | JWT Bearer | 9.x |
| JWT lib | `System.IdentityModel.Tokens.Jwt` | 8.x |
| Hash de password | BCrypt.Net-Next (no MD5) | última |
| Mapping | AutoMapper + `ProjectTo` | 12.x |
| Documentación API | Swashbuckle + Annotations + Filters | 10.x |
| Health Checks | `AspNetCore.HealthChecks.SqlServer` | 9.x |
| `.env` | DotNetEnv | 3.x |
| Email | MailKit (SMTP) | última |

**Excluidos a propósito**: MediatR, FluentValidation, Mapster, Hangfire, Serilog, ASP.NET Core Identity. No agregar sin discutir antes.

### Frontend

| Componente | Tecnología | Versión |
|---|---|---|
| Framework | Vue 3 (`<script setup lang="ts">`) | 3.5+ |
| Lenguaje | TypeScript | 5.x |
| Bundler | Vite + `vite-plugin-vue-devtools` | 5+ |
| Estado | Pinia (setup syntax) | 3.x |
| Router | Vue Router | 4.x |
| HTTP | Axios + interceptores auth/refresh | 1.x |
| Server-state | `@tanstack/vue-query` | 5.x |
| Formularios | vee-validate + `@vee-validate/zod` | 4.x |
| Validación | Zod 4 (errorMap global en español) | 4.x |
| Utilities | `@vueuse/core`, `clsx`, `tailwind-merge`, `class-variance-authority` | — |
| Iconos | `lucide-vue-next` | última |
| UI | shadcn-vue (style "new-york", baseColor neutral) + `reka-ui` | 2.x |
| CSS | Tailwind CSS 4 vía `@tailwindcss/vite` + `tw-animate-css` | 4.x |
| Lint/Format | oxlint + eslint + prettier | — |
| Package manager | pnpm | (Node ^20.19 \|\| ≥22.12) |

**Excluidos a propósito**: PrimeVue, Vuetify, otros UI kits.

## 2. Estructura backend

```
Back/
├── Backend.sln
│
├── Backend.Api/                ← Web API
│   ├── Controllers/            ← BaseApiController + AuthController + ...
│   ├── Filters/                ← ValidationFilter
│   ├── Middleware/             ← ExceptionMiddleware
│   ├── Program.cs              ← composition root
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── Backend.Application/        ← Use cases, DTOs, interfaces
│   ├── Auth/
│   │   ├── Commands/  (+ Examples/)
│   │   ├── DTOs/      (+ Examples/)
│   │   └── UseCases/
│   ├── Common/
│   │   ├── DTOs/               ← ApiResponse<T>, ApiError, Pagination
│   │   ├── Exceptions/         ← BusinessException
│   │   ├── Extensions/         ← QueryableExtensions (WhereIf, SearchBy)
│   │   ├── Interfaces/         ← ICryptoService, ITokenService, IEmailService
│   │   └── Persistence/        ← ICdtDbContext
│   └── <Dominio>/              ← carpeta por dominio del negocio
│       ├── DTOs/, Profiles/ (AutoMapper), Queries/, UseCases/
│
├── Backend.Domain/             ← Entities sin dependencias externas
│   └── Entities/
│       ├── Base/AuditableEntity.cs
│       ├── Auth/               ← Usuario, RefreshToken, TokenResetPassword
│       └── <Dominio>/
│
└── Backend.Infrastructure/     ← Implementaciones de servicios y persistencia
    ├── Common/
    │   ├── Persistence/        ← CdtDbContext (implementa ICdtDbContext)
    │   ├── Services/           ← CryptoService, TokenService, EmailService,
    │                              ExpiredRefreshTokenCleanupService (IHostedService)
    └── Persistence/Configurations/  ← IEntityTypeConfiguration<T> por entidad
```

**Referencias entre proyectos**:
- `Backend.Application` → `Backend.Domain`
- `Backend.Infrastructure` → `Backend.Application` (y transitivamente Domain)
- `Backend.Api` → `Backend.Application` + `Backend.Infrastructure`

## 3. Pipeline de respuesta — `ApiResponse<T>`

Toda respuesta sigue un único contrato:

```csharp
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public Pagination? Pagination { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Data = data, Success = true };
    public static ApiResponse<T> Ok(T data, Pagination pagination) =>
        new() { Data = data, Pagination = pagination, Success = true };
    public static ApiResponse<T> Fail(string message, string code, object? details = null) =>
        new() { Success = false, Message = message, Error = new ApiError(code, details) };
}

public class ApiError(string code, object? details)
{
    public string Code { get; set; } = code;
    public object? Details { get; set; } = details;
}

public class Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalRecords { get; set; }
}
```

## 4. `BaseApiController`

```csharp
[ApiController]
public class BaseApiController : ControllerBase
{
    protected IActionResult ApiOk<T>(T data) => Ok(ApiResponse<T>.Ok(data));
    protected IActionResult ApiOk<T>(T data, Pagination p) => Ok(ApiResponse<T>.Ok(data, p));
    protected IActionResult ApiFail<T>(string message, string code, object? details = null)
        => BadRequest(ApiResponse<T>.Fail(message, code, details));
}
```

## 5. Pipeline de errores

```
Request
  ↓
ValidationFilter (global, registrado en AddControllers)
  ├─ ModelState inválido → ApiResponse.Fail("Datos de entrada inválidos",
  │                                          "VALIDATION_ERROR", { campo: [errores] })
  └─ válido
       ↓
Controller / UseCase
  ├─ throw new BusinessException("USER_NOT_FOUND", "El usuario no existe")
  ├─ throw Exception (cualquier otra)
  └─ return ApiOk(...)
       ↓
ExceptionMiddleware
  ├─ BusinessException → 400 + ApiResponse.Fail con su Code y Details
  └─ Exception        → 500 + ApiResponse.Fail("Error interno", "UNKNOWN_ERROR")
       ↓
Response JSON (camelCase, omite nulos)
```

## 6. Convenciones de Use Case

- Una clase por use case en `Backend.Application/<Dominio>/UseCases/`.
- Constructor inyecta dependencias (DbContext, services).
- Único método público `ExecuteAsync(command, cancellationToken = default)`.
- Sin lógica HTTP. Devuelve DTO o lanza `BusinessException`.

```csharp
public class LoginUseCase
{
    private readonly ICdtDbContext _context;
    private readonly ICryptoService _crypto;
    private readonly ITokenService _tokens;

    public LoginUseCase(ICdtDbContext context, ICryptoService crypto, ITokenService tokens)
    {
        _context = context;
        _crypto = crypto;
        _tokens = tokens;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginCommand command, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == command.Correo, ct);
        if (usuario is null || !_crypto.VerifyPassword(command.Password, usuario.ClaveHash))
            throw new BusinessException("LOGIN_FAILED", "Correo o contraseña incorrectos");

        if (!usuario.Estado)
            throw new BusinessException("USER_INACTIVE", "Usuario inactivo");

        var token = _tokens.GenerateJwtToken(usuario);
        var refresh = _tokens.GenerateRefreshToken();
        // ... persiste refresh, devuelve DTO
    }
}
```

## 7. Convenciones de Command / DTO

- **Commands**: clases públicas, sin lógica. Validación con `[Required]`, `[StringLength]`, `[EmailAddress]`, `[SwaggerSchema(...)]`.
- **DTOs**: pensados para respuesta; con `[SwaggerSchema(Example = "...")]`.
- **XML docs obligatorias**: `<summary>`, `<param>`, `<returns>` en miembros públicos. `<GenerateDocumentationFile>true</GenerateDocumentationFile>` en `Backend.Api.csproj`.
- **Examples**: `IExamplesProvider<T>` por DTO/Command, registrados con `AddSwaggerExamplesFromAssemblies`.

## 8. Convenciones de Controller

- Hereda de `BaseApiController`.
- `[ApiController]`, `[Route("api/[controller]")]`, `[Produces("application/json")]`.
- Inyección por constructor de los UseCase.
- Cada acción anotada con `[HttpPost("login")]`, `[ProducesResponseType(typeof(Dto), Status200OK)]` por status posible, `[SwaggerRequestExample(...)]` y `[SwaggerResponseExample(...)]`.
- La acción solo orquesta: `await useCase.ExecuteAsync(...)` y `return ApiOk(result)`.

## 9. Persistencia

- **Una sola interfaz** `ICdtDbContext` declarada en `Application/Common/Persistence/` con todos los `DbSet<T>` y `SaveChangesAsync(CancellationToken)`.
- Implementada por `CdtDbContext` en `Infrastructure/Common/Persistence/`.
- `ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())` aplica todos los `IEntityTypeConfiguration<T>`.
- Queries de lectura: **`AsNoTracking()`** + **`ProjectTo<Dto>(mapper.ConfigurationProvider)`**.
- Filtros condicionales: `WhereIf(condition, predicate)` y `SearchBy(term, columnSelector)` en `QueryableExtensions`.

## 10. AutoMapper

- Cada dominio tiene su `*Profile` en `Backend.Application/<Dominio>/Profiles/`.
- Registro: `builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())`.

## 11. Auth (JWT + Refresh)

- **Login**: verifica usuario por correo → verifica password con BCrypt → verifica `Estado`/`fechaConfirmacionEmail`.
- Genera **JWT (8 h)** + **refresh token random (7 d)** + persiste el refresh.
- **Refresh endpoint**: valida ambos tokens y emite nuevos.
- **Logout**: invalida el refresh token (marca `revocado = true`).
- **`ExpiredRefreshTokenCleanupService` (`IHostedService`)** limpia tokens vencidos diariamente.
- **Cookies**: `httpOnly`, `Secure`, `SameSite=Strict` para el refresh token; el access token vive en memoria del front.

## 12. `Program.cs` — composition root

Secciones numeradas y comentadas:
1. `Env.Load()`.
2. Swagger: XML, annotations, examples.
3. `AddControllers` con `ValidationFilter` global y `AddJsonOptions` (camelCase, omitir nulos). `ConfigureApiBehaviorOptions.InvalidModelStateResponseFactory = ValidationFilter.BuildResponse`.
4. `AddDbContext<CdtDbContext>(... UseSqlServer(...))`.
5. DI: `ICryptoService`, `ITokenService`, `IEmailService`, todos los UseCase como `Scoped`. `AddAutoMapper(...)`. `AddHostedService<ExpiredRefreshTokenCleanupService>()`. `AddHealthChecks().AddSqlServer(...)`.
6. `AddAuthentication(JwtBearer)` con `TokenValidationParameters`.
7. `AddAuthorization()`.
8. `AddCors("PermitirFrontend")` con `WithOrigins(...)` apuntando al puerto del front.
9. Pipeline: Swagger (dev) → `UseHttpsRedirection` → `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapHealthChecks("/health")` → `MapControllers` → `UseMiddleware<ExceptionMiddleware>()`.

## 13. `appsettings.json` — esqueleto

```json
{
  "ConnectionStrings": {
    "Cdt": "Server=...;Database=CDT;User Id=...;Password=...;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "<256-bit secret>",
    "Issuer": "ORF",
    "Audience": "CDT",
    "ExpiryInHours": 8
  },
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "User": "",
    "Password": "",
    "From": "no-reply@orf.local",
    "EnableSsl": false
  },
  "Frontend": {
    "BaseUrl": "http://localhost:5173"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

## 14. Entity base

```csharp
public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    public int IdUsuarioCreacion { get; set; }
    public int IdUsuarioModificacion { get; set; }
}
```

## 15. Estructura frontend

```
Front/
├── src/
│   ├── main.ts                       ← Pinia + Router + VueQuery
│   ├── App.vue                       ← solo <router-view />
│   ├── assets/main.css               ← Tailwind 4 entrypoint
│   ├── components/                   ← componentes globales mínimos
│   ├── components/ui/                ← shadcn-vue generado
│   ├── composables/                  ← transversales (useTheme.ts)
│   ├── config/env.ts                 ← única puerta a import.meta.env
│   ├── lib/
│   │   ├── api.ts                    ← axios instance + interceptors
│   │   ├── HttpClient.ts             ← class HttpClient por recurso
│   │   └── utils.ts                  ← cn() helper
│   ├── modules/                      ← UN MÓDULO POR FEATURE
│   │   └── <feature>/
│   │       ├── adapters/             ← funciones puras Api → Front
│   │       ├── components/
│   │       ├── composables/
│   │       ├── schemas/              ← Zod + toTypedSchema
│   │       ├── services/             ← class XxxService { client = new HttpClient(...) }
│   │       ├── stores/               ← Pinia setup-stores
│   │       ├── types/                ← XxxApi (espejo backend) + Xxx (front)
│   │       ├── views/
│   │       └── index.ts              ← barrel
│   ├── router/index.ts               ← rutas + guard global
│   ├── types/api.ts                  ← ApiResponse<T>, ApiError, Pagination
│   ├── utils/                        ← dateUtils, errorMapper
│   ├── validation/
│   │   ├── errorMap.ts               ← Zod 4 customError global (español)
│   │   └── fields/                   ← stringField, passwordField, emailField, ...
│   └── views/                        ← solo lo cross-feature (HomeView)
│
├── components.json                   ← shadcn-vue config
├── env.d.ts
├── eslint.config.ts
├── vite.config.ts                    ← alias '@' → ./src
└── .env example                      ← VITE_API_BASE_URL=...
```

## 16. Bootstrap (`main.ts`)

```ts
import './assets/main.css'
import '@/validation/errorMap'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'

import App from './App.vue'
import router from './router'

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 1000 * 60 * 5, retry: 1 } },
})

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.use(VueQueryPlugin, { queryClient })
app.mount('#app')
```

## 17. Variables de entorno — única puerta

```ts
// src/config/env.ts
function getEnvVar(name: string, value: string | undefined, defaultValue?: string): string {
  if (value) return value
  if (defaultValue !== undefined) return defaultValue
  throw new Error(`FATAL: La variable de entorno ${name} no está definida y no tiene default.`)
}

export const ENV = {
  API_BASE_URL: getEnvVar('VITE_API_BASE_URL', import.meta.env.VITE_API_BASE_URL),
  API_PREFIX:   getEnvVar('VITE_API_PREFIX',   import.meta.env.VITE_API_PREFIX,   '/api'),
  APP_NAME:     getEnvVar('VITE_APP_NAME',     import.meta.env.VITE_APP_NAME,     'CDT'),
  IS_PROD:      import.meta.env.PROD,
} as const
```

**Regla**: nunca `import.meta.env` fuera de este archivo.

## 18. Cliente HTTP

- `src/lib/api.ts`: instancia axios con `baseURL: ENV.API_BASE_URL`, `withCredentials: true`. Interceptor de request inyecta `Bearer <token>`. Interceptor de response: si **401 + no es retry**, intenta `authService.refreshToken()` (con cola para peticiones concurrentes), reintenta la original; si falla redirige a `/login`.
- `src/lib/HttpClient.ts`: clase con `resourcePath` (ej. `/auth`). Métodos `get/post/put/patch/delete` que construyen `${ENV.API_PREFIX}${resourcePath}${endpoint}`, validan `response.data.success`, unifican errores con `extractErrorMessage`.

## 19. Patrón de módulo

1. **types**: `XxxApi` (espejo del backend) + `Xxx` (modelo del frontend). Sin `any`.
2. **adapters**: función pura `xxxAdapter(data: XxxApi): Xxx` por entidad. Sin lógica de negocio.
3. **services**: `class XxxService { private readonly client = new HttpClient('/recurso') }`. Devuelven **siempre tipos del frontend** (invocando al adapter). Export default singleton.
4. **schemas**: zod + `toTypedSchema` para vee-validate. Reusar builders de `src/validation/fields/`.
5. **stores** (si aplica): Pinia setup-store. State como `ref()`, getters como `computed()`, acciones como funciones.
6. **composables**:
   - `useXxx()` — operaciones del dominio. Expone `isLoading`, `error`, funciones.
   - `useXxxForm()` — orquesta `useForm` de vee-validate con el schema.
7. **components**: piezas visuales del módulo.
8. **views**: pantallas.
9. **index.ts** (barrel): re-exports públicos del módulo.

## 20. Validación con Zod 4 + vee-validate

- `errorMap` global en `src/validation/errorMap.ts` (mensajes en español por código de issue de Zod).
- Builders en `src/validation/fields/`:
  - `stringField({ label, min, max, required })`
  - `passwordField`, `emailField`, `numberField`, `phoneField`, `urlField`, `selectField`, `booleanField`, `dateField`.
- Schema de feature:
  ```ts
  export const loginSchema = z.object({
    correo:   emailField({ label: 'correo' }),
    password: passwordField({ label: 'contraseña', min: 6, max: 100 }),
  })
  export type LoginFormValues = z.infer<typeof loginSchema>
  export const loginFormSchema = toTypedSchema(loginSchema)
  ```

## 21. Manejo de errores en frontend

`extractErrorMessage` aplica esta jerarquía:
1. `response.data.message` del backend.
2. `response.data.error.code` → `Error técnico: <code>`.
3. `Network Error` → "No se pudo establecer conexión con el servidor...".
4. `ECONNABORTED` → "La solicitud tardó demasiado tiempo...".
5. `error.message` genérico.
6. Fallback: "Ocurrió un error inesperado. Por favor, contacta al soporte."

## 22. Naming y idioma

- **Dominio en español**: entidades, propiedades, mensajes de error de negocio (`Usuario`, `Tarea`, `IdUsuarioCreacion`, `Estado`, `Prioridad`).
- **Infra/técnicos en inglés**: `Pagination`, `ApiResponse`, `HttpClient`, `useTheme`.
- **JSON serializado**: camelCase (configurado globalmente).
- **Tipos del API en frontend**: sufijo `Api` (`LoginResponseApi`); modelos del frontend sin sufijo (`AuthTokens`).
- **Códigos de error**: `SCREAMING_SNAKE_CASE`.

## 23. Catálogo de error codes

```
# Validación / sistema
VALIDATION_ERROR
UNKNOWN_ERROR

# Auth
LOGIN_FAILED
REGISTER_FAILED
EMAIL_ALREADY_EXISTS
EMAIL_NOT_CONFIRMED
EMAIL_CONFIRMATION_FAILED
RESET_TOKEN_INVALID
RESET_TOKEN_EXPIRED
REFRESH_TOKEN_FAILED
LOGOUT_FAILED
USER_NOT_FOUND
USER_INACTIVE

# Permisos
FORBIDDEN

# Dominio
EQUIPO_NOT_FOUND
PROYECTO_NOT_FOUND
LISTA_NOT_FOUND
TAREA_NOT_FOUND
INVALID_PARENT_TASK
ESTADO_NOT_FOUND
PRIORIDAD_NOT_FOUND
ETIQUETA_NOT_FOUND
INVITACION_INVALID
INVITACION_EXPIRED
```

Se amplía conforme aparecen casos de negocio.

## 24. Conventional commits

```
type(scope): descripción en español

- Cambio 1
- Cambio 2
```

Types: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`, `perf`, `style`. Scopes: `api`, `ui`, `auth`, `tareas`, `equipos`, `docs`, etc. Tipo y scope en inglés; descripción y body en español.

## 25. Comandos de setup

### Backend (desde `Back/`)

```powershell
dotnet new sln -n Backend
dotnet new webapi    -n Backend.Api
dotnet new classlib  -n Backend.Application
dotnet new classlib  -n Backend.Domain
dotnet new classlib  -n Backend.Infrastructure
dotnet sln add Backend.Api Backend.Application Backend.Domain Backend.Infrastructure

dotnet add Backend.Application    reference Backend.Domain
dotnet add Backend.Infrastructure reference Backend.Application
dotnet add Backend.Api            reference Backend.Application Backend.Infrastructure

# Paquetes principales
dotnet add Backend.Api package Swashbuckle.AspNetCore
dotnet add Backend.Api package Swashbuckle.AspNetCore.Annotations
dotnet add Backend.Api package Swashbuckle.AspNetCore.Filters
dotnet add Backend.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add Backend.Api package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add Backend.Api package DotNetEnv
dotnet add Backend.Api package AspNetCore.HealthChecks.SqlServer
dotnet add Backend.Api package Microsoft.AspNetCore.Diagnostics.HealthChecks

dotnet add Backend.Application package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add Backend.Application package Microsoft.EntityFrameworkCore
dotnet add Backend.Application package Swashbuckle.AspNetCore.Annotations
dotnet add Backend.Application package Swashbuckle.AspNetCore.Filters

dotnet add Backend.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add Backend.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add Backend.Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add Backend.Infrastructure package BCrypt.Net-Next
dotnet add Backend.Infrastructure package MailKit
```

### Frontend (desde `Front/`)

```powershell
pnpm create vite . --template vue-ts
pnpm add vue-router pinia axios @tanstack/vue-query
pnpm add vee-validate @vee-validate/zod zod
pnpm add @vueuse/core class-variance-authority clsx tailwind-merge
pnpm add lucide-vue-next reka-ui
pnpm add -D tailwindcss @tailwindcss/vite tw-animate-css
pnpm add -D oxlint eslint @typescript-eslint/parser @typescript-eslint/eslint-plugin eslint-plugin-vue eslint-plugin-oxlint @vue/eslint-config-typescript eslint-config-prettier prettier
pnpm add -D vue-tsc @vue/tsconfig @types/node
pnpm add -D vite-plugin-vue-devtools npm-run-all2 jiti shadcn-vue
pnpm dlx shadcn-vue@latest init   # style: new-york, baseColor: neutral, cssVariables: true
```
