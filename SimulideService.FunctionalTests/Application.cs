using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimulideService.Domain.Data;
using SimulideService.FunctionalTests.DB;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests;

[SetUpFixture]
public class Application
{
    public static IAlbaHost? Host { get; private set; }
    
    [OneTimeSetUp]
    public async Task Initialize()
    {
        Host = await AlbaHost.For<Program>(x =>
        {
            x.UseEnvironment("Test");
            x.ConfigureServices((context, services) =>
            {
                services.AddScoped<IStatusRepository, MockSuccessStatusRepository>();
            });
        });
    }
    
    
    [OneTimeTearDown]
    public void TearDown()
    {
        Host?.Dispose();
    }
}