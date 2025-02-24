using SimulideService.Domain;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IStatusRepository
{
    Task<Either<Exception, bool>> IsHealthy();
}

public class StatusRepository(CollabContext dbContext) : IStatusRepository
{
    public Task<Either<Exception, bool>> IsHealthy()
    {
        try
        {
            var result = dbContext.Documents.Count() >= 0;
            return Task.FromResult(new Either<Exception, bool>(result));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new Either<Exception, bool>(ex));
        }
    }
}