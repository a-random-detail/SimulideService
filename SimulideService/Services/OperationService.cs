using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Validators;

namespace SimulideService.Services;

public interface IOperationService 
{
   Task<Either<List<Exception>, Operation>> ApplyOperationAsync(ApplyOperationPayload request, Document document);
}

public class OperationService(
   IOperationWriteRepository operationWriteRepository,
   ITransactionManager<CollabContext> transactionManager,
   ILogger<OperationService> logger) : IOperationService
{
   public async Task<Either<List<Exception>, Operation>> ApplyOperationAsync(ApplyOperationPayload request, Document document)
   {
      return await ApplyOperationPayloadValidator.FieldsAreValid(request, document)
         .Bind(Operation.FromRequest)
         .BindAsync(operation => transactionManager.ExecuteInTransaction(async (dbContext) =>
         {
            var appliedOperation = await operationWriteRepository.CreateAsync(dbContext, operation);
            logger.LogInformation($"Operation {appliedOperation.Id} created.");
            await operationWriteRepository.ApplyOperationToDocument(dbContext, appliedOperation, document);
            logger.LogInformation($"Operation {appliedOperation.Id} applied to document {document.Id}.");
            return appliedOperation;
         }));
   }
}