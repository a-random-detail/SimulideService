using SimulideService.Domain.Contracts;

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

    public static Operation FromRequest(ApplyOperationPayload payload)
    {
        var currentDateTime = DateTime.UtcNow;
        return new Operation
        {
            DocumentId = payload.DocumentId,
            Type = payload.Type,
            Position = payload.Position,
            Content = payload.Content,
            Version = payload.Version,
            Length = payload.Length,
            CreatedAt = currentDateTime
        };
    }
}

public enum OperationType
{
    None,
    Insert,
    Delete
}