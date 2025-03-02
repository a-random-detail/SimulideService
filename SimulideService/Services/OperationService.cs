using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Validators;

namespace SimulideService.Services;

public interface IOperationService 
{
   Task<Either<List<Exception>, Operation>> ApplyOperationAsync(ApplyOperationPayload request, Document document, CancellationToken cancellationToken);
}

public class OperationService(
   IOperationWriteRepository operationWriteRepository, 
   ITransactionManager<CollabContext> transactionManager): IOperationService 
{
   public async Task<Either<List<Exception>, Operation>> ApplyOperationAsync(ApplyOperationPayload request, Document document, CancellationToken cancellationToken)
   {
      return await ApplyOperationPayloadValidator.FieldsAreValid(request, document)
         .Bind(Operation.FromRequest)
         .BindAsync(operation => transactionManager.ExecuteInTransaction(async (dbContext) => await operationWriteRepository.CreateAsync(dbContext, operation)));
   }
}