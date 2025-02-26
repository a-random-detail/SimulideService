namespace SimulideService.Domain;

public class Either<TL, TR>
{
    private readonly TL _left = default!;
    private readonly TR _right = default!;
    private readonly bool _isLeft;
    
    public static implicit operator Either<TL, TR>(TL left) => new(left);
    public static implicit operator Either<TL, TR>(TR right) => new(right);
    
    public static Either<TL, TR> Left(TL left) => new(left);
    public static Either<TL, TR> Right(TR right) => new(right);

    public Either(TL left)
    {
        _left = left;
        _isLeft = true;
    }

    public Either(TR right)
    {
        _right = right;
        _isLeft = false;
    }

    public T Match<T>(Func<TL, T> error, Func<TR, T> success)
    {
        return _isLeft ? error(_left) : success(_right);
    }
    
    public async Task<Either<TL, TResult>> MapAsync<TResult>(Func<TR, Task<Either<TL, TResult>>> func)
    {
        return _isLeft ? _left : await func(_right);
    }
    
    public Either<TL, TResult> Map<TResult>(Func<TR, TResult> func)
    {
        return _isLeft ? _left : func(_right);
    }
    
}
