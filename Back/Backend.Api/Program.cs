using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Api.Filters;
using Backend.Api.Middleware;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

// 1. Cargar variables de entorno desde .env (si existe)
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// 2. Swagger / OpenAPI (sin Bearer todavía — se agrega cuando se wire JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.ExampleFilters();

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

// 4. AutoMapper (descubre profiles en assemblies cargados)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 5. CORS
const string CorsPolicy = "PermitirFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// 6. Health checks
builder.Services.AddHealthChecks();

// TODO (próxima fase): DbContext, JWT auth con Bearer en Swagger, registros DI de servicios.

var app = builder.Build();

// Pipeline
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
