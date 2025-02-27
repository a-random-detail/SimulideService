namespace SimulideService.Domain.Contracts;

public class PostDocumentRequest
{
    public required String Name { get; set; }
    public string? Content { get; set; }
}