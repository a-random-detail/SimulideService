using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IDocumentWriteRepository
{
   Task<Document> CreateAsync(CollabContext dbContext, Document document); 
}

public class DocumentWriteRepository : IDocumentWriteRepository
{
   public async Task<Document> CreateAsync(CollabContext dbContext, Document document)
   {
      dbContext.Documents.Add(document);
      await dbContext.SaveChangesAsync();
      return document;
   }
}
