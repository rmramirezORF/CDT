using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Api.Filters;
using Backend.Api.Middleware;
using Backend.Application.Admin.UseCases;
using Backend.Application.Auth.UseCases;
using Backend.Application.Common.Interfaces;
using Backend.Application.Equipos.UseCases;
using Backend.Application.Listas.UseCases;
using Backend.Application.Proyectos.UseCases;
using Backend.Application.Tareas.UseCases;
using Backend.Application.Common.Persistence;
using Backend.Infrastructure.Common.Persistence;
using Backend.Infrastructure.Common.Services;
using Backend.Infrastructure.Persistence.Seeders;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.Filters;

// 1. Cargar variables de entorno desde .env (si existe)
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// 2. Swagger / OpenAPI (con Bearer auth para probar endpoints protegidos desde la UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Doc name "cdt" en vez del clásico "v1" → fuerza al browser a pedir una URL nueva
    // que no tiene en cache (evita el caso donde un /swagger/v1/swagger.json viejo
    // sin "openapi" quedó persistido en disco/SW).
    c.SwaggerDoc("cdt", new OpenApiInfo
    {
        Title = "CDT API",
        Version = "v1",
        Description = "Control de Tareas (CDT) — API interna ORF.",
    });

    c.EnableAnnotations();
    c.ExampleFilters();

    // Bearer auth para Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. Pega solo el token (sin la palabra 'Bearer').",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetExecutingAssembly());

// 3. Controllers + JSON options + ValidationFilter
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ValidationFilter.BuildResponse;
});

// 4. AutoMapper — escanea solo el assembly de Application (donde viven los profiles).
//    Evita usar AppDomain.CurrentDomain.GetAssemblies() porque tocaria DLLs externos
//    (ej. Swashbuckle/OpenApi) y puede romper el inicio si hay versiones inconsistentes.
builder.Services.AddAutoMapper(typeof(ICdtDbContext).Assembly);

// 5. DbContext
var connectionString = builder.Configuration.GetConnectionString("Cdt")
    ?? throw new InvalidOperationException("ConnectionStrings:Cdt no configurado");

builder.Services.AddDbContext<CdtDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICdtDbContext>(sp => sp.GetRequiredService<CdtDbContext>());

// 6. JWT Bearer auth
var jwtKey      = builder.Configuration["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key no configurado");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? throw new InvalidOperationException("Jwt:Issuer no configurado");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

// 7. DI de servicios de Infrastructure
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 7.1. DI de Use Cases — Auth
builder.Services.AddScoped<RegisterUseCase>();
builder.Services.AddScoped<ConfirmEmailUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RefreshTokenUseCase>();
builder.Services.AddScoped<LogoutUseCase>();
builder.Services.AddScoped<ForgotPasswordUseCase>();
builder.Services.AddScoped<ResetPasswordUseCase>();

// 7.2. DI de Use Cases — Admin
builder.Services.AddScoped<ListarUsuariosUseCase>();
builder.Services.AddScoped<CambiarRolUsuarioUseCase>();
builder.Services.AddScoped<CambiarEstadoUsuarioUseCase>();
builder.Services.AddScoped<EliminarUsuarioUseCase>();
builder.Services.AddScoped<ConfirmarEmailManualmenteUseCase>();
builder.Services.AddScoped<EstadosCatalogoUseCases>();
builder.Services.AddScoped<PrioridadesCatalogoUseCases>();
builder.Services.AddScoped<EtiquetasCatalogoUseCases>();
builder.Services.AddScoped<TiposActividadCatalogoUseCases>();
builder.Services.AddScoped<DominiosPermitidosUseCases>();

// 7.3. DI de Use Cases — Equipos
builder.Services.AddScoped<ListarEquiposUseCase>();
builder.Services.AddScoped<ObtenerEquipoUseCase>();
builder.Services.AddScoped<CrearEquipoUseCase>();
builder.Services.AddScoped<ActualizarEquipoUseCase>();
builder.Services.AddScoped<EliminarEquipoUseCase>();
builder.Services.AddScoped<AgregarMiembroUseCase>();
builder.Services.AddScoped<QuitarMiembroUseCase>();

// 7.4. DI de Use Cases — Proyectos
builder.Services.AddScoped<ListarProyectosUseCase>();
builder.Services.AddScoped<ObtenerProyectoUseCase>();
builder.Services.AddScoped<CrearProyectoUseCase>();
builder.Services.AddScoped<ActualizarProyectoUseCase>();
builder.Services.AddScoped<EliminarProyectoUseCase>();

// 7.5. DI de Use Cases — Listas
builder.Services.AddScoped<ListarListasUseCase>();
builder.Services.AddScoped<ObtenerListaUseCase>();
builder.Services.AddScoped<CrearListaUseCase>();
builder.Services.AddScoped<ActualizarListaUseCase>();
builder.Services.AddScoped<EliminarListaUseCase>();
builder.Services.AddScoped<ReordenarListasUseCase>();

// 7.6. DI de Use Cases — Tareas
builder.Services.AddScoped<ListarTareasUseCase>();
builder.Services.AddScoped<ObtenerTareaUseCase>();
builder.Services.AddScoped<CrearTareaUseCase>();
builder.Services.AddScoped<ActualizarTareaUseCase>();
builder.Services.AddScoped<EliminarTareaUseCase>();

// 8. CORS
const string CorsPolicy = "PermitirFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// 9. Health checks (con SQL Server)
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sqlserver");

var app = builder.Build();

// Aplica migraciones pendientes y seedea catálogos al startup (idempotente).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CdtDbContext>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");
    db.Database.Migrate();
    await CatalogosSeeder.SeedAsync(db);
    // En desarrollo, también seedeamos datos demo (60 usuarios + equipos + proyectos + tareas).
    if (app.Environment.IsDevelopment())
    {
        await DemoSeeder.SeedAsync(db, seederLogger);
    }
}

// Pipeline
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Middleware: el SwaggerUI bundleado con Swashbuckle 6.6.2 no rendiza openapi 3.0.4
    // (la version que emite Microsoft.OpenApi 1.6.23). Lo "downgradeamos" a 3.0.1 en vuelo
    // antes de servir el JSON. Cosmético: la spec real sigue siendo 3.0.x compatible.
    app.Use(async (ctx, next) =>
    {
        if (!ctx.Request.Path.StartsWithSegments("/swagger") || !ctx.Request.Path.Value!.EndsWith(".json"))
        {
            await next();
            return;
        }

        var originalBody = ctx.Response.Body;
        using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;

        await next();

        ctx.Response.Body = originalBody;
        buffer.Seek(0, SeekOrigin.Begin);
        var content = await new StreamReader(buffer).ReadToEndAsync();
        content = content.Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.1\"");

        ctx.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(content);
        ctx.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        await ctx.Response.WriteAsync(content);
    });

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        // URL ABSOLUTA con / inicial → SwaggerUI hace early-return sin pasar por el replace
        // defectuoso que ocurre con paths relativos.
        c.SwaggerEndpoint("/swagger/cdt/swagger.json", "CDT API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "CDT API — Swagger";
    });

    // Scalar — UI alternativa moderna en /scalar (sin el legacy de SwaggerUI).
    app.MapScalarApiReference(options =>
    {
        options.Title = "CDT API";
        options.OpenApiRoutePattern = "/swagger/cdt/swagger.json";
    });
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
