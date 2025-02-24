using SimulideService.Domain;
using SimulideService.Repositories;

namespace SimulideService.FunctionalTests.DB;

public class MockSuccessStatusRepository: IStatusRepository
{
    public Task<Either<Exception, bool>> IsHealthy()
    {
        return Task.FromResult(new Either<Exception, bool>(true));
    }
}

public class MockFailingStatusRepository : IStatusRepository
{
    public Task<Either<Exception, bool>> IsHealthy()
    {
        return Task.FromResult(new Either<Exception, bool>(new Exception("DB connection failed")));
    }
}
