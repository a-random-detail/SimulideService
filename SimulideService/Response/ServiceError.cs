using System.Net;

namespace SimulideService.Response;

public class ServiceError
{
    public string Message { get; set; }
    public ServiceLayer Layer { get; set; }
}
