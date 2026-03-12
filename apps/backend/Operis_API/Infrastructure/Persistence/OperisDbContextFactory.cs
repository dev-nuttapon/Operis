using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Operis_API.Shared.Configuration;

namespace Operis_API.Infrastructure.Persistence;

public sealed class OperisDbContextFactory : IDesignTimeDbContextFactory<OperisDbContext>
{
    public OperisDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationManager();
        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonFile("appsettings.json", optional: true);
        configuration.AddJsonFile("appsettings.Development.json", optional: true);
        configuration.AddEnvironmentVariables();
        configuration.ApplyDatabaseUrlOverride();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<OperisDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new OperisDbContext(optionsBuilder.Options);
    }
}
