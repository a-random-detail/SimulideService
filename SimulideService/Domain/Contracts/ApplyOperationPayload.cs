using SimulideService.Domain.Data;

namespace SimulideService.Domain.Contracts;

public class ApplyOperationPayload 
{
    public required Guid DocumentId { get; init; }
    public required OperationType Type { get; init; }
    public int Position { get; init; }
    public string? Content { get; init; }
    public int Version { get; init; }
    public int Length { get; init; }
    
    public DateTime CreatedAt { get; set; }
}