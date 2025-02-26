using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SimulideService.FunctionalTests.DB;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests;

[TestFixture]
public class HealthCheckTest
{
    [Test]
    public async Task Healthcheck_Returns_OK()
    {
        await Application.Host.Scenario( _ =>
        {
            _.Get.Url("/healthcheck");
            _.StatusCodeShouldBeOk();
        });
    }

    [Test]
    public async Task Healthcheck_Returns_BadGateway_OnError()
    {
        var errorHost = await AlbaHost.For<Program>(x =>
        {
            x.UseEnvironment("Test");
            x.ConfigureServices((context, services) =>
            {
                services.AddScoped<IStatusRepository, MockFailingStatusRepository>();
            });
        });
        
        await errorHost.Scenario( _ =>
        {
            _.Get.Url("/healthcheck");
            _.StatusCodeShouldBe(HttpStatusCode.BadGateway);
        });
    }

}
