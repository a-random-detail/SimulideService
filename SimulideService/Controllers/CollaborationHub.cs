using System.Net;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Repositories.Queries;
using SimulideService.Response;
using SimulideService.Services;
using SimulideService.Validators;
using static SimulideService.Response.ServiceResponse<SimulideService.Domain.Data.Operation>;
using static SimulideService.Response.ServiceResponse<SimulideService.Domain.CollabUserEvent>;

namespace SimulideService.Controllers;

public class CollaborationHub(
    IOperationService operationService, 
    IMediator mediator,
    IWebSocketManager webSocketManager,
    ILogger<CollaborationHub> logger): Hub
{

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug($"Client disconnected: {Context.ConnectionId}");
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogDebug($"Client connected: {Context.ConnectionId}");
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    [HubMethodName("ApplyOperation")]
    public async Task<ServiceResponse<Operation>> ApplyOperation(ApplyOperationPayload request)
    {
        return await (await mediator
                .SendToEitherAsync(new GetDocumentByIdQuery(request.DocumentId),
                    () => new KeyNotFoundException($"Document with id {request.DocumentId} not found.")))
            .BindAsync(doc => operationService.ApplyOperationAsync(request, doc))
            .BindAsync(op => webSocketManager.BroadcastOperation(op, request.DocumentId))
            .MatchAsync<List<Exception>, Operation, ServiceResponse<Operation>>(
                error: (exceptions) => Task.FromResult(HandleErrors(exceptions)),
                success: (operation) => Task.FromResult(SuccessResult(HttpStatusCode.OK, operation)));

    }


    [HubMethodName("JoinDocumentGroup")]
    public async Task<ServiceResponse<CollabUserEvent>> JoinDocumentGroup(Guid documentId)
    {
        var documentResult = await FetchDocumentByIdAsync(documentId);
        return await documentResult 
            .BindAsync(doc => webSocketManager.JoinDocumentGroup(documentId, Context.ConnectionId))
            .MatchAsync<List<Exception>, CollabUserEvent, ServiceResponse<CollabUserEvent>>(
                error: exceptions => Task.FromResult(HandleUserErrors(exceptions)),
                success: evt => Task.FromResult(SuccessResult(HttpStatusCode.OK, evt)));
    }


    [HubMethodName("LeaveDocumentGroup")]
    public async Task<ServiceResponse<CollabUserEvent>> LeaveDocumentGroup(Guid documentId, CancellationToken cancellationToken = default)
    {
        var documentResult = await FetchDocumentByIdAsync(documentId);
        return await documentResult 
            .BindAsync(doc => webSocketManager.LeaveDocumentGroup(documentId, Context.ConnectionId))
            .MatchAsync<List<Exception>, CollabUserEvent, ServiceResponse<CollabUserEvent>>(
                error: exceptions => Task.FromResult(HandleUserErrors(exceptions)),
                success: evt => Task.FromResult(SuccessResult(HttpStatusCode.OK, evt)));
    }

    private async Task<Either<List<Exception>, Document>> FetchDocumentByIdAsync(Guid documentId)
    {
        return await mediator
            .SendToEitherAsync(new GetDocumentByIdQuery(documentId),
                () => new KeyNotFoundException($"Document with id {documentId} not found."));
    }
    
    
    private static ServiceResponse<Operation> HandleErrors(List<Exception> exceptions)
    {
        if (exceptions.Any(x => x is OperationValidationException))
            return ServiceResponse<Operation>.ErrorResult(HttpStatusCode.BadRequest,
                exceptions.Select(ex => new ServiceError { Message = ex.Message }).ToList());
        if (exceptions.Any(x => x is KeyNotFoundException)) 
            return ServiceResponse<Operation>.ErrorResult(HttpStatusCode.NotFound, [ new ServiceError { Message = exceptions.First().Message }]);
        
        return ServiceResponse<Operation>.ErrorResult(HttpStatusCode.InternalServerError,
                exceptions.Select(ex => new ServiceError { Message = "An error occurred while processing the request." }).ToList());
    }
    
    private static ServiceResponse<CollabUserEvent> HandleUserErrors(List<Exception> exceptions)
    {
        if (exceptions.Any(x => x is KeyNotFoundException)) 
            return ServiceResponse<CollabUserEvent>.ErrorResult(HttpStatusCode.NotFound, [ new ServiceError { Message = exceptions.First().Message }]);
        
        return ServiceResponse<CollabUserEvent>.ErrorResult(HttpStatusCode.InternalServerError,
                exceptions.Select(ex => new ServiceError { Message = "An error occurred while processing the request." }).ToList());
    }
    
    
}