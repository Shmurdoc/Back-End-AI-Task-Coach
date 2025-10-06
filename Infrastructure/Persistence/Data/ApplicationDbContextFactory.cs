using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Data;

/// <summary>
/// Design-time factory for ApplicationDbContext to support EF Core migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Build configuration to read from appsettings.json
        var basePath = Directory.GetCurrentDirectory();
        
        // Check if we're in the Infrastructure project directory, if so, go up to WebAPI
        if (basePath.EndsWith("Infrastructure"))
        {
            basePath = Path.Combine(basePath, "../WebAPI");
        }
        // Check if we're in the solution root, then go to WebAPI
        else if (Directory.Exists(Path.Combine(basePath, "WebAPI")))
        {
            basePath = Path.Combine(basePath, "WebAPI");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
