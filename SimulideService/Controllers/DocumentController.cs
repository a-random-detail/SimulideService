using System.Net;
using Microsoft.AspNetCore.Mvc;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Response;
using SimulideService.Services;
using static SimulideService.Response.ServiceResponse<SimulideService.Domain.Data.Document>;

namespace SimulideService.Controllers;

[ApiController]
[Route("/documents")]
public class DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostDocumentRequest document)
    {
        var result = await documentService.CreateDocumentAsync(document);
        return result.Match<ServiceResponse<Document>>(
            error: exs => ErrorResult(HttpStatusCode.BadRequest,  exs.Select(x => new ServiceError { Message = x.Message} ).ToList()),
            success: doc => SuccessResult(HttpStatusCode.Created, doc))
            .ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        throw new NotImplementedException();
    }
}
