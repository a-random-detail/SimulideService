using System.Data;
using Dapper;
using SimulideService.Domain;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IDocumentReadRepository
{
   Task<Either<Exception, Document>> GetDocumentByIdAsync(Guid documentId); 
}

public class DocumentReadRepository(IDbConnection dbConnection, ILogger<DocumentReadRepository> logger) : IDocumentReadRepository
{
   private const string GetDocumentByIdQuery = $@"SELECT * FROM Documents WHERE DocumentId = @DocumentId";
   
   public async Task<Either<Exception, Document>> GetDocumentByIdAsync(Guid documentId)
   {
      try
      {
         var document = await dbConnection.QueryFirstOrDefaultAsync<Document>(GetDocumentByIdQuery, new { DocumentId = documentId });
         
         return document is not null
            ? Either<Exception, Document>.Right(document)
            : Either<Exception, Document>.Left(new KeyNotFoundException($"Document with id {documentId} not found"));
      }
      catch (Exception ex)
      {
         logger.LogError("Error getting document by id", ex);
         return Either<Exception, Document>.Left(ex);
      }
   }
}