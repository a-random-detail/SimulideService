namespace SimulideService.Domain.Data;

public class Operation
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public OperationType Type { get; set; }
    public UInt32 Position { get; set; }
    public String? Content { get; set; }
    public UInt32 Version { get; set; }
    public UInt32 Length { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum OperationType
{
    Insert,
    Delete
}