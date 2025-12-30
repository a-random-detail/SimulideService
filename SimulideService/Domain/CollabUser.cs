namespace SimulideService.Domain;

public class CollabUserEvent
{
    public string ConnectionId { get; set; }
    public PartyActionType Action { get; set; }

    public List<CollabUser> ActiveUsers { get; set; } = new();
}

public class CollabUser
{
    public string UserId { get; set; }
    public int Position { get; set; }
}

public enum PartyActionType
{
    Join,
    Leave
}