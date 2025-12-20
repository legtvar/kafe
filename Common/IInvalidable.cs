namespace Kafe;

public interface IInvalidable
{
    bool IsValid { get; }
}

public interface IInvalidable<T> : IInvalidable
    where T : IInvalidable
{
    static abstract T Invalid { get; }
}
