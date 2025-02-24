namespace SimulideService.Domain.Data;

public class Document
{
    public Guid Id { get; set; }
    public String Name { get; set; }
    public String? Content { get; set; }
    public UInt32 Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}