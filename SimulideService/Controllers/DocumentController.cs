using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Repositories.Queries;
using SimulideService.Response;
using SimulideService.Services;
using SimulideService.Validators;
using static SimulideService.Response.ServiceResponse<SimulideService.Domain.Data.Document>;

namespace SimulideService.Controllers;

[ApiController]
[Route("/documents")]
public class DocumentController(IDocumentService documentService, IMediator mediator, ILogger<DocumentController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostDocumentRequest document)
    {
        var result = await documentService.CreateDocumentAsync(document);
        return result.Match<ServiceResponse<Document>>(
            error: HandleDocumentErrors,
            success: doc => SuccessResult(HttpStatusCode.Created, doc))
            .ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDocumentByIdQuery(id), cancellationToken);
        return result.Match<ServiceResponse<Document>>(
            error: e => ErrorResult(HttpStatusCode.NotFound, [
                new ServiceError { Message = e.Message }
                    //{ Message = "Document not found" }
            ]),
            success: doc => SuccessResult(HttpStatusCode.OK, doc))
            .ToActionResult();
    }
    
    private static ServiceResponse<Document> HandleDocumentErrors(List<Exception> exceptions)
    {
        if (exceptions.Any(x => x is DocumentValidationException))
            return ErrorResult(HttpStatusCode.BadRequest,
                exceptions.Select(ex => new ServiceError { Message = ex.Message }).ToList());
        
        return ErrorResult(HttpStatusCode.InternalServerError,
                exceptions.Select(ex => new ServiceError { Message = ex.Message }).ToList());
    }
}
