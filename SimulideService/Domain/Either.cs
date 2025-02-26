namespace SimulideService.Domain;

public class Either<L, R>
{
    private readonly L _left;
    private readonly R _right;
    private readonly bool _isLeft;
    
    public static implicit operator Either<L, R>(L left) => new(left);
    public static implicit operator Either<L, R>(R right) => new(right);
    
    public static Either<L, R> Left(L left) => new(left);
    public static Either<L, R> Right(R right) => new(right);

    public Either(L left)
    {
        _left = left;
        _isLeft = true;
    }

    public Either(R right)
    {
        _right = right;
        _isLeft = false;
    }

    public T Match<T>(Func<L, T> error, Func<R, T> success)
    {
        return _isLeft ? error(_left) : success(_right);
    }
    
    public async Task<Either<L, TResult>> MapAsync<TResult>(Func<R, Task<Either<L, TResult>>> func)
    {
        return _isLeft ? _left : await func(_right);
    }
    
    public Either<L, TResult> Map<TResult>(Func<R, TResult> func)
    {
        return _isLeft ? _left : func(_right);
    }
    
}
