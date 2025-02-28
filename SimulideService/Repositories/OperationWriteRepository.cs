using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IOperationWriteRepository
{
   Task<Operation> CreateAsync(CollabContext dbContext, Operation operation); 
}

public class OperationWriteRepository : IOperationWriteRepository 
{
   public async Task<Operation> CreateAsync(CollabContext dbContext, Operation operation)
   {
      dbContext.Operations.Add(operation);
      await dbContext.SaveChangesAsync();
      return operation;
   }
}
