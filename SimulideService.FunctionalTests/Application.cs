using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SimulideService.FunctionalTests.DB;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests;

[SetUpFixture]
public class Application
{
    [OneTimeSetUp]
    public async Task Initialize()
    {
        Host = await AlbaHost.For<Program>(x =>
        {
            x.ConfigureServices((context, services) =>
            {
                services.AddScoped<IStatusRepository, MockSuccessStatusRepository>();
            });
        });
        
    }
    
    public static IAlbaHost Host { get; private set; }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        Host?.Dispose();
    }
}
