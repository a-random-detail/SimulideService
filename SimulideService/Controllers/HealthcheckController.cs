using System.Net;
using Microsoft.AspNetCore.Mvc;
using SimulideService.Domain;
using SimulideService.Repositories;
using SimulideService.Response;
using static SimulideService.Response.ServiceResponse<SimulideService.Domain.ServiceStatus>;

namespace SimulideService.Controllers;

[ApiController]
[Route("/healthcheck")]
public class HealthcheckController(IStatusRepository statusRepository, ILogger<HealthcheckController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return (await statusRepository.IsHealthy())
            .Match<ServiceResponse<ServiceStatus>>(
                error: _ => ErrorResult(HttpStatusCode.BadGateway, [
                    new ServiceError
                    {
                        Message = "The database is not healthy"
                    }
                ]),
                success: isHealthy => SuccessResult(HttpStatusCode.OK, new ServiceStatus
                {
                    DatabaseIsHealthy = isHealthy
                })).ToActionResult();
    }
}