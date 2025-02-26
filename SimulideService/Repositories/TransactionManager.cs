using Microsoft.EntityFrameworkCore;
using SimulideService.Domain;

namespace SimulideService.Repositories;

public interface ITransactionManager<C> where C : DbContext
{
   Task<Either<List<Exception>, T>> ExecuteInTransaction<T>(Func<C, Task<T>> action); 
}

public class TransactionManager<C>(C dbContext): ITransactionManager<C> where C : DbContext
{
   public async Task<Either<List<Exception>, T>> ExecuteInTransaction<T>(Func<C, Task<T>> action)
   {
      await using var transaction = await dbContext.Database.BeginTransactionAsync();

      try
      {
         var result = await action(dbContext);
         await dbContext.SaveChangesAsync();
         await transaction.CommitAsync();
         return Either<List<Exception>, T>.Right(result);
      }
      catch (Exception ex)
      {
         await transaction.RollbackAsync();
         return Either<List<Exception>, T>.Left([ex]);
      }
   }
}
