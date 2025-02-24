using SimulideService.Domain;

namespace SimulideService.Repositories;

public interface IStatusRepository
{
    Task<Either<Exception, bool>> IsHealthy();
}

public class StatusRepository: IStatusRepository
{
    public Task<Either<Exception, bool>> IsHealthy()
    {
        throw new NotImplementedException();
    }
}