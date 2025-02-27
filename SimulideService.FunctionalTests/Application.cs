using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using Testcontainers.PostgreSql;

namespace SimulideService.FunctionalTests;

[SetUpFixture]
public class Application
{
    public static IAlbaHost? Host { get; private set; }
    public static PostgreSqlContainer? PostgreSqlContainer { get; private set; }

    [OneTimeSetUp]
    public async Task Initialize()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14.6-alpine")
            .WithHostname("localhost")
            .WithDatabase("simulide-db")
            .WithUsername("user")
            .WithPassword("password")
            .WithCleanUp(true)
            .Build();
        
        await PostgreSqlContainer.StartAsync();
        var testConnectionString = PostgreSqlContainer.GetConnectionString();
        Host = await AlbaHost.For<Program>(x =>
        {
            x.UseEnvironment("Test");

            x.ConfigureServices((context, services) =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CollabContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<CollabContext>(options =>
                    options.UseNpgsql(testConnectionString));
                services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(testConnectionString));
                services.AddScoped<IDocumentReadRepository, DocumentReadRepository>();
            });
        });
        using var scope = Host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();
        await dbContext.Database.MigrateAsync();
        
        // await EnsureTableExists(testConnectionString);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Host!.DisposeAsync();
        await PostgreSqlContainer!.StopAsync();
        await PostgreSqlContainer.DisposeAsync();
    }
    
    private async Task EnsureTableExists(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var checkTableCmd = new NpgsqlCommand("SELECT to_regclass('public.documents');", conn);
        var result = await checkTableCmd.ExecuteScalarAsync();

        if (result == DBNull.Value || result == null)
        {
            throw new InvalidOperationException("❌ ERROR: 'documents' table does not exist after migration!");
        }
    }
}