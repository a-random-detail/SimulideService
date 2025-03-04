using System.Diagnostics;

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
    
    public Either<TL, TResult> Bind<TResult>(Func<TR, TResult> func)
    {
        return _isLeft ? _left : func(_right);
    }
    
    public async Task<Either<TL, TResult>> BindAsync<TResult>(Func<TR, Task<Either<TL, TResult>>> func)
    {
        return _isLeft ? Either<TL, TResult>.Left(_left) : await func(_right);
    }

    public T Match<T>(Func<TL, T> error, Func<TR, T> success)
    {
        return _isLeft ? error(_left) : success(_right);
    }
    
    public async Task<TResult> MatchAsync<TResult>(
        Func<TL, Task<TResult>> error, 
        Func<TR, Task<TResult>> success)
     {
         return _isLeft ? await error(_left) : await success(_right);
     }
}
