using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Validators;

namespace SimulideService.Services;

public interface IDocumentService
{
   Task<Either<List<Exception>, Document>> CreateDocumentAsync(PostDocumentRequest request);
}

public class DocumentService(
   IDocumentWriteRepository documentWriteRepository, 
   ITransactionManager<CollabContext> transactionManager): IDocumentService
{
   public async Task<Either<List<Exception>, Document>> CreateDocumentAsync(PostDocumentRequest request)
   {
      return await CreateDocumentRequestValidator.FieldsAreValid(request)
         .Map(Document.FromRequest)
         .MapAsync(document => transactionManager.ExecuteInTransaction(async (dbContext) => await documentWriteRepository.CreateAsync(dbContext, document)));
   }
}