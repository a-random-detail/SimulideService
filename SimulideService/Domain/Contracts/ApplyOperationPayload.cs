using SimulideService.Domain.Data;

namespace SimulideService.Domain.Contracts;

public class ApplyOperationPayload 
{
    public Guid DocumentId { get; set; }
    public OperationType Type { get; set; }
    public UInt32 Position { get; set; }
    public String? Content { get; set; }
    public UInt32 Version { get; set; }
    public UInt32 Length { get; set; }
    public DateTime CreatedAt { get; set; }
}