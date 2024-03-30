using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Kafe.Common;

/// <summary>
/// An error union type. Can be used to return exceptions instead of throwing them.
/// </summary>
public readonly record struct Err<T>
{
    // Inspired in part by: https://stackoverflow.com/questions/3151702/discriminated-union-in-c-sharp
    // And: https://ziglang.org/documentation/master/#while-with-Error-Unions

    private readonly T value;
    private readonly ImmutableArray<Error> errors;

    public Err()
    {
        value = default!;
        errors = ImmutableArray<Error>.Empty;
    }

    public Err(T value) : this()
    {
        this.value = value;
    }

    public Err(ImmutableArray<Error> errors) : this()
    {
        this.errors = errors;
    }

    public Err(Error error) : this(ImmutableArray.Create(error))
    {
    }

    public Err(Exception exception) : this()
    {
        var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
        errors = [new Error(exception, stackTrace.ToString())];
    }

    public T Value => value;

    public ImmutableArray<Error> Errors => errors;

    public bool HasErrors => !errors.IsEmpty;


    public static implicit operator Err<T>(T value)
    {
        return new Err<T>(value);
    }

    public static implicit operator Err<T>(Error error)
    {
        return new Err<T>(error);
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
