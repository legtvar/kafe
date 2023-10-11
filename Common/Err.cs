using System.Collections.Immutable;

namespace Kafe.Common;

/// <summary>
/// An error union type. Can be used to return exceptions comfortably instead of throwing them.
/// </summary>
public readonly record struct Err<T>
{
    private readonly T value;
    private readonly ImmutableArray<Exception> exceptions;

    public Err()
    {
        value = default!;
        exceptions = ImmutableArray<Exception>.Empty;
    }

    public Err(T value) : this()
    {
        this.value = value;
    }

    public Err(ImmutableArray<Exception> exceptions) : this()
    {
        this.exceptions = exceptions;
    }

    public Err(Exception exception) : this(ImmutableArray.Create(exception))
    {
    }

    public T? Value => value;

    public ImmutableArray<Exception> Exceptions => exceptions;

    public bool HasErrors => !exceptions.IsEmpty;

    public static implicit operator Err<T>(T value)
    {
        return new Err<T>(value);
    }
    
    public static implicit operator Err<T>(Exception exception)
    {
        return new Err<T>(exception);
    }

    public static implicit operator T?(Err<T> err)
    {
        return err.HasErrors ? default : err.value;
    }
}
