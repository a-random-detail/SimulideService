using SimulideService.Domain.Data;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests.DB;

public class ErrorDocumentRepository: IDocumentRepository
{
    public Task<Domain.Data.Document> CreateAsync(CollabContext dbContext, Domain.Data.Document document)
    {
        throw new Exception("Database error"); 
    }
}