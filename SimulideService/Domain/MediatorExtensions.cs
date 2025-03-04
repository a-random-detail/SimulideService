using MediatR;
using SimulideService.Domain;

public static class MediatorExtensions
{
    public async static Task<Either<List<Exception>, T>> SendToEitherAsync<T>(
        this IMediator mediator,
        IRequest<Either<List<Exception>, T>> request,
        Func<Exception> onNone,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var result = await mediator.Send(request, cancellationToken);
            return result.Match(
                error: Either<List<Exception>, T>.Left,
                success: Either<List<Exception>, T>.Right);
        }
        catch (OperationCanceledException)
        {
            return Either<List<Exception>, T>.Left([new OperationCanceledException("The operation was canceled.")]);
        }
        catch (Exception ex)
        {
            return Either<List<Exception>, T>.Left([onNone(), ex]);
        }
    }
}