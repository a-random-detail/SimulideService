namespace SimulideService.Domain;

public class CollabUserEvent
{
    public string ConnectionId { get; set; }
    public PartyActionType Action { get; set; }
}

public enum PartyActionType
{
    Join,
    Leave
}