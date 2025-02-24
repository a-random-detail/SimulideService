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
        var result = response as OkObjectResult;
        

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

        var response = await _healthcheckController.Get() as StatusCodeResult;
        var result = response.StatusCode as int?; 
        
        Assert.That(result, Is.EqualTo((int)HttpStatusCode.BadGateway));
    }
    
}