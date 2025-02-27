using SimulideService.Domain;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests;

public class InMemoryDocumentReadRepository : IDocumentReadRepository
{
    private readonly List<Domain.Data.Document> _documents = new();

    public Task AddAsync(Domain.Data.Document document)
    {
        _documents.Add(document);
        return Task.CompletedTask;
    }

    public Task<Either<Exception, Domain.Data.Document>> GetDocumentByIdAsync(Guid documentId)
    {
        var result = _documents.FirstOrDefault(d => d.Id == documentId);
        return result is not null 
            ? Task.FromResult(Either<Exception, Domain.Data.Document>.Right(result))
            : Task.FromResult(Either<Exception, Domain.Data.Document>.Left(new KeyNotFoundException($"Document with id {documentId} not found")));
    }
}