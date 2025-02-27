using SimulideService.Domain;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests.Document;

public class ErrorDocumentReadRepository: IDocumentReadRepository
{
    public Task<Either<Exception, Domain.Data.Document>> GetDocumentByIdAsync(Guid documentId)
    {
        return Task.FromResult(Either<Exception, Domain.Data.Document>.Left(new Exception("womp womp")));
    }
}