namespace SimulideService.Domain;

public class Either<L, R>
{
    private readonly L Left;
    private readonly R Right;
    private readonly bool IsLeft;
    
    public static implicit operator Either<L, R>(L left) => new(left);
    public static implicit operator Either<L, R>(R right) => new(right);

    public Either(L left)
    {
        Left = left;
        IsLeft = true;
    }

    public Either(R right)
    {
        Right = right;
        IsLeft = false;
    }

    public T Match<T>(Func<L, T> error, Func<R, T> success)
    {
        return IsLeft ? error(Left) : success(Right);
    }
    
}
