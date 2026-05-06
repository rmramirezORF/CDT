using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Common.Persistence;

/// <summary>
/// Permite a EF Core (dotnet ef migrations / database update) construir el DbContext
/// sin bootear toda la app — evita conflictos transitivos como el de Microsoft.OpenApi
/// y permite generar migraciones aunque la app aun no resuelva DI completa.
/// </summary>
public class CdtDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CdtDbContext>
{
    public CdtDbContext CreateDbContext(string[] args)
    {
        // Working dir cuando se invoca `dotnet ef ... -p Backend.Infrastructure`
        // es la carpeta de Backend.Infrastructure. Subimos un nivel y entramos a Backend.Api.
        var apiPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Backend.Api"));

        var config = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Cdt")
            ?? "Server=localhost;Database=CDT;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<CdtDbContext>()
            .UseSqlServer(connectionString);

        return new CdtDbContext(optionsBuilder.Options);
    }
}
