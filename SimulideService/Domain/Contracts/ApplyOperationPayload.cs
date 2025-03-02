using SimulideService.Domain.Data;

namespace SimulideService.Domain.Contracts;

public class ApplyOperationPayload 
{
    public required Guid DocumentId { get; init; }
    public required OperationType Type { get; init; }
    public uint Position { get; init; }
    public string? Content { get; init; }
    public uint Version { get; init; }
    public uint Length { get; init; }
    
    public DateTime CreatedAt { get; set; }
}