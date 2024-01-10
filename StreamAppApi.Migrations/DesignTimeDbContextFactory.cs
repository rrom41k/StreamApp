using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using StreamAppApi.Bll.DbConfiguration;

namespace StreamAppApi.Migrations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StreamPlatformDbContext>
{
    public StreamPlatformDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<StreamPlatformDbContext>();

        var connectionString = configuration.GetConnectionString("StreamApp");

        builder.UseNpgsql(connectionString, b => 
            b.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));

        return new StreamPlatformDbContext(builder.Options);
    }
}