using System.Data;
using Dapper;
using SimulideService.Domain;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IDocumentReadRepository
{
   Task<Either<List<Exception>, Document>> GetDocumentByIdAsync(Guid documentId); 
}

public class DocumentReadRepository(IDbConnection dbConnection, ILogger<DocumentReadRepository> logger) : IDocumentReadRepository
{
   private const string GetDocumentByIdQuery = "SELECT * FROM \"Documents\" WHERE \"Id\" = @DocumentId";
   
   public async Task<Either<List<Exception>, Document>> GetDocumentByIdAsync(Guid documentId)
   {
      try
      {
         var document = await dbConnection.QueryFirstOrDefaultAsync<Document>(GetDocumentByIdQuery, new { DocumentId = documentId });
         
         return document is not null
            ? Either<List<Exception>, Document>.Right(document)
            : Either<List<Exception>, Document>.Left([new KeyNotFoundException($"Document with id {documentId} not found.")]);
      }
      catch (Exception ex)
      {
         logger.LogError(ex, "Error getting document {DocumentId}. Exception Type: {ExceptionType}",
            documentId, ex.GetType().Name);
         return Either<List<Exception>, Document>.Left([ex]);
      }
   }
}