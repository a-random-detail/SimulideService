using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimulideService.Controllers;
using SimulideService.Domain;
using SimulideService.Repositories;
using SimulideService.Response;

namespace SimulideService.UnitTests.Healthcheck;

[TestFixture]
public class HealthcheckController_Tests
{
    private HealthcheckController _healthcheckController;
    private readonly Mock<IStatusRepository> _statusRepository = new();
    private readonly Mock<ILogger<HealthcheckController>> _logger = new();

    [SetUp]
    public void SetUp()
    {
        _healthcheckController = new HealthcheckController(_statusRepository.Object, _logger.Object);
    }

    [Test]
    public async Task Get_ReturnsOk_WhenDbStatusIsHealthy()
    {
        _statusRepository
            .Setup(x => x.IsHealthy())
            .ReturnsAsync(new Either<Exception, bool>(true));

        var response = await _healthcheckController.Get();
        var result = response as ObjectResult;
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        var payload = result.Value as ServiceResponse<ServiceStatus>;
        Assert.IsTrue(payload!.Success);
        Assert.IsTrue(payload.Data!.DatabaseIsHealthy);
    }
    
    [Test]
    public async Task Get_ReturnsError_WhenDbStatusIsUnhealthy()
    {
        _statusRepository
            .Setup(x => x.IsHealthy())
            .ReturnsAsync(new Either<Exception, bool>(
                new Exception("DB connection failed")));

        var response = await _healthcheckController.Get() as ObjectResult;
        var result = response!.Value as ServiceResponse<ServiceStatus>; 
        
        Assert.That(response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadGateway));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Errors!.First().Message, Is.EqualTo("The database is not healthy"));
    }
    
}