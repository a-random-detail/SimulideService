using MediatR;

namespace SimulideService.Domain;

public static class EitherExtensions
{
    public static Either<TL, TR> ToEither<TL, TR>(this TR? value, Func<TL> onNull)
        where TR : class
    {
        return value is not null 
            ? Either<TL, TR>.Right(value) 
            : Either<TL, TR>.Left(onNull());
    }
    
    public async static Task<Either<TL, TR>> ToEitherAsync<TL, TR>(
        this Task<TR?> task,
        Func<TL> onNull)
        where TR : class
    {
        var result = await task;
        return result is not null ? Either<TL, TR>.Right(result) : Either<TL, TR>.Left(onNull());
    }
    
    public async static Task<Either<TL, TR>> BindAsync<TL, TR>(this Task<Either<TL, TR>> eitherTask, Func<TR, Task<Either<TL, TR>>> func)
    {
        var either = await eitherTask;
        return await either.BindAsync(func);
    }
    
    public async static Task<TResult> MatchAsync<TL, TR, TResult>(
        this Task<Either<TL, TR>> eitherTask,
        Func<TL, Task<TResult>> error, 
        Func<TR, Task<TResult>> success)
    {
        var either = await eitherTask;
        return await either.MatchAsync(error, success);
    }
    
    public async static Task<Either<List<Exception>, T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            var result = await func();
            return Either<List<Exception>, T>.Right(result);
        }
        catch (Exception ex)
        {
            return Either<List<Exception>, T>.Left([ex]);
        }
    }
    
    public async static Task<Either<List<Exception>, Unit>> TryAsync(Func<Task> func)
    {
        try
        {
            await func();
            return Either<List<Exception>, Unit>.Right(Unit.Value);
        }
        catch (Exception ex)
        {
            return Either<List<Exception>, Unit>.Left([ex]);
        }
    }
}