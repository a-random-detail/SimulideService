using Microsoft.AspNetCore.SignalR;
using SimulideService.Controllers;
using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;

namespace SimulideService.Services;

public interface IWebSocketManager
{
    Task<Either<List<Exception>, Operation>> BroadcastOperation(Operation operation, Guid documentId);
    Task<Either<List<Exception>, CollabUserEvent>> JoinDocumentGroup(Guid documentId, string connectionId);
    Task<Either<List<Exception>, CollabUserEvent>> LeaveDocumentGroup(Guid documentId, string connectionId);
}

public class WebSocketManager(IHubContext<CollaborationHub> hubContext): IWebSocketManager
{
    private readonly IHubContext<CollaborationHub> _hubContext = hubContext;
    
    public async Task<Either<List<Exception>, Operation>> BroadcastOperation(Operation operation, Guid documentId)
    {

        await _hubContext.Clients.Group(documentId.ToString())
            .SendAsync("ReceiveOperation", operation);
        return Either<List<Exception>, Operation>.Right(operation);
    }
    
    public async Task<Either<List<Exception>, CollabUserEvent>> JoinDocumentGroup(Guid documentId, string connectionId)
    {
        try
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, documentId.ToString());
            var collabUserEvent = new CollabUserEvent
            {
                Action = PartyActionType.Join,
                ConnectionId = connectionId
            };
            
            await _hubContext.Clients.Group(documentId.ToString())
                .SendAsync("PartyChanged", collabUserEvent);
            
            return Either<List<Exception>, CollabUserEvent>.Right(collabUserEvent);
        }
        catch (Exception ex)
        {
            return Either<List<Exception>, CollabUserEvent>.Left([ex]);
        }
    }
    
    public async Task<Either<List<Exception>, CollabUserEvent>> LeaveDocumentGroup(Guid documentId, string connectionId)
    {
        try
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, documentId.ToString());
            var collabUserEvent = new CollabUserEvent
            {
                Action = PartyActionType.Leave,
                ConnectionId = connectionId
            };
            
            await _hubContext.Clients.Group(documentId.ToString())
                .SendAsync("PartyChanged", collabUserEvent);
            
            return Either<List<Exception>, CollabUserEvent>.Right(collabUserEvent);
        } catch (Exception e)
        {
            return Either<List<Exception>, CollabUserEvent>.Left([e]);
        }
    }

}