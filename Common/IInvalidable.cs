namespace Kafe;

public interface IInvalidable
{
    bool IsValid { get; }

    public static bool GetIsValid(object? obj)
    {
        return obj is not null && (obj is not IInvalidable invalidable || invalidable.IsValid);
    }
}

public interface IInvalidable<out T> : IInvalidable
    where T : IInvalidable<T>
{
    static abstract T Invalid { get; }
}
