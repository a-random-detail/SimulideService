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
    public static CollabContext? DbContext { get; private set; }
    public static PostgreSqlContainer? PostgreSqlContainer { get; private set; }

    [OneTimeSetUp]
    public async Task Initialize()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
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
                
                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();
                dbContext.Database.Migrate();
            });
        });

        // Create a DbContext for manual data setup in tests
        using var scope = Host.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Host!.DisposeAsync();
        await DbContext!.DisposeAsync();
        await PostgreSqlContainer!.StopAsync();
        await PostgreSqlContainer.DisposeAsync();
    }
}