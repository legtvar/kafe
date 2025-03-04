using System.Diagnostics.CodeAnalysis;

namespace Kafe;

public interface IMergeable
{
    bool TryMergeWith(object other, [NotNullWhen(true)] out object? merged);
}

public interface IMergable<T> : IMergeable
{
    bool TryMergeWith(T other, [NotNullWhen(true)] out object? merged);
}
