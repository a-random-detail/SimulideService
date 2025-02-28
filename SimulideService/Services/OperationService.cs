using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Validators;

namespace SimulideService.Services;

public interface IOperationService 
{
   Task<Either<List<Exception>, Operation>> ApplyOperation(ApplyOperationPayload request, Document document);
}

public class OperationService(
   IOperationWriteRepository operationWriteRepository, 
   ITransactionManager<CollabContext> transactionManager): IOperationService 
{
   public async Task<Either<List<Exception>, Operation>> ApplyOperation(ApplyOperationPayload request, Document document)
   {
      
      return await ApplyOperationPayloadValidator.FieldsAreValid(request, document)
         .Map(Operation.FromRequest)
         .MapAsync(document => transactionManager.ExecuteInTransaction(async (dbContext) => await operationWriteRepository.CreateAsync(dbContext, document)));
   }
}