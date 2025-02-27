using SimulideService.Domain.Data;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests.DB;

public class ErrorDocumentWriteRepository: IDocumentWriteRepository
{
    public Task<Domain.Data.Document> CreateAsync(CollabContext dbContext, Domain.Data.Document document)
    {
        throw new Exception("Database error"); 
    }
}