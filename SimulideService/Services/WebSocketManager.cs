using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SimulideService.Controllers;
using SimulideService.Domain;
using SimulideService.Domain.Data;

namespace SimulideService.Services;

public interface IWebSocketManager
{
    Task<Either<List<Exception>, Operation>> BroadcastOperation(Operation operation, Guid documentId, string senderConnectionId);
    Task<Either<List<Exception>, CollabUserEvent>> JoinDocumentGroup(Guid documentId, string connectionId);
    Task<Either<List<Exception>, CollabUserEvent>> LeaveDocumentGroup(Guid documentId, string connectionId);
    Task HandleDisconnection(string connectionId);
}

public class WebSocketManager(IHubContext<CollaborationHub> hubContext, ILogger<WebSocketManager> logger): IWebSocketManager
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CollabUser>> ActiveUsersByDocument = new();
    
    public async Task<Either<List<Exception>, Operation>> BroadcastOperation(Operation operation, Guid documentId, string senderConnectionId )
    {
        var docKey = documentId.ToString();
        
        logger.LogInformation("[WebSocketManager] Broadcasting operation {OperationId} to document {DocumentId} from sender {SenderConnectionId}", operation.Id, documentId, senderConnectionId);
        return await EitherExtensions.TryAsync(async () =>
        {
            await hubContext.Clients
                .GroupExcept(docKey, new[] { senderConnectionId })
                .SendAsync("ReceiveOperation", operation);
            logger.LogInformation("[WebSocketManager] Operation {OperationId} broadcasted to document {DocumentId}",
                operation.Id, docKey);
            return operation;
        });
    }
    
    public async Task<Either<List<Exception>, CollabUserEvent>> JoinDocumentGroup(Guid documentId, string connectionId)
    {
        var docKey = documentId.ToString();
        try
        {
            var user = new CollabUser
            {
                UserId = connectionId,
                Position = 0
            };
            await hubContext.Groups.AddToGroupAsync(connectionId, documentId.ToString());
            
            var documentUsers = ActiveUsersByDocument.GetOrAdd(docKey, _ => new ConcurrentDictionary<string, CollabUser>());
            
            logger.LogInformation("[WebSocketManager] User {ConnectionId} joining document {DocumentId}. Current users: {CurrentUsersCount}", connectionId, documentId, documentUsers.Count);
            documentUsers.AddOrUpdate(connectionId, user, (key, oldValue) => user);
            
            logger.LogInformation("[WebSocketManager] User {ConnectionId} joined document {DocumentId}. Updated users: {UpdatedUsersCount}", connectionId, documentId, documentUsers.Count);
            
            var activeUsers = documentUsers.Values.ToList();
            
            var collabUserEvent = new CollabUserEvent
            {
                Action = PartyActionType.Join,
                ConnectionId = connectionId,
                ActiveUsers = activeUsers
            };
            
            await hubContext.Clients.Group(docKey)
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
        var docKey = documentId.ToString();
        try
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, docKey);
            var collabUserEvent = new CollabUserEvent
            {
                Action = PartyActionType.Leave,
                ConnectionId = connectionId,
            };
            
            if (ActiveUsersByDocument.TryGetValue(docKey, out var users))
            {
                users.TryRemove(connectionId, out var _);
                collabUserEvent.ActiveUsers = users.Values.ToList();
            }
            else
            {
                collabUserEvent.ActiveUsers = [];
            }
            
            await hubContext.Clients.Group(docKey)
                .SendAsync("PartyChanged", collabUserEvent);
            
            return Either<List<Exception>, CollabUserEvent>.Right(collabUserEvent);
        } catch (Exception e)
        {
            return Either<List<Exception>, CollabUserEvent>.Left([e]);
        }
    }

    public async Task HandleDisconnection(string connectionId)
    {
        foreach (var (documentId, users) in ActiveUsersByDocument)
        {
            logger.LogInformation($"[WebSocketManager] Handling disconnection for connection {connectionId} in document {documentId}");
            ActiveUsersByDocument[documentId].TryRemove(connectionId, out var _);

            var activeUsers = users.Values.ToList();
            
            var collabUserEvent = new CollabUserEvent
            {
                Action = PartyActionType.Leave,
                ConnectionId = connectionId,
                ActiveUsers = activeUsers 
            };
            
            await hubContext.Clients.Group(documentId.ToString())
                .SendAsync("PartyChanged", collabUserEvent);
        }
    }
}