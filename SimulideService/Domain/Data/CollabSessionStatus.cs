namespace SimulideService.Domain.Data;

public class CollabSessionStatus
{
    public Guid DocumentId { get; set; }
    public bool IsConnected { get; set; }
    public string ConnectionId { get; set; }
}