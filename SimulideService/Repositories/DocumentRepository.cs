using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IDocumentRepository
{
   Task<Document> CreateAsync(CollabContext dbContext, Document document); 
}

public class DocumentRepository : IDocumentRepository
{
   public async Task<Document> CreateAsync(CollabContext dbContext, Document document)
   {
      dbContext.Documents.Add(document);
      await dbContext.SaveChangesAsync();
      return document;
   }
}
