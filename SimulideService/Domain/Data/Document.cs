using System.ComponentModel.DataAnnotations;
using SimulideService.Domain.Contracts;

namespace SimulideService.Domain.Data;

public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required String Name { get; set; }
    public String? Content { get; set; }
    public UInt32 Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public static Document FromRequest(PostDocumentRequest request)
    {
        var currentDateTime = DateTime.UtcNow;
        return new Document
        {
            Name = request.Name,
            Content = request.Content,
            Version = 1,
            CreatedAt = currentDateTime,
            UpdatedAt = currentDateTime
        };
    }
}