using MediatR;
using Microsoft.AspNetCore.SignalR;
using SimulideService.Domain.Contracts;
using SimulideService.Repositories.Queries;
using SimulideService.Services;

namespace SimulideService.Controllers;

public class CollaborationHub(IOperationService operationService, IMediator mediator): Hub
{
    public async Task ApplyOperation(ApplyOperationPayload request, CancellationToken cancellationToken)
    {
        var document = await mediator.Send(new GetDocumentByIdQuery(id), cancellationToken);
        var result = await operationService.ApplyOperation(request);
        result.Match(error: list => , success: operation => );
        if (result.IsLeft)
        {
            throw new Exception("An error occurred while processing the request.");
        }
        await Clients.All.SendAsync("ReceiveOperation", result.Right);
    }
    
    
}