using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Kafe;

/// <summary>
/// An error union type. Can be used to return exceptions instead of throwing them.
/// </summary>
public readonly record struct Err<T>
{
    // Inspired in part by: https://stackoverflow.com/questions/3151702/discriminated-union-in-c-sharp
    // And: https://ziglang.org/documentation/master/#while-with-Error-Unions

    private readonly T value;
    private readonly ImmutableArray<Diagnostic> errors;

    public Err()
    {
        value = default!;
        errors = ImmutableArray<Diagnostic>.Empty;
    }

    public Err(T value) : this(value, [])
    {
    }

    public Err(ImmutableArray<Diagnostic> errors) : this(default!, errors)
    {
    }

    public Err(Diagnostic error) : this([error])
    {
    }

    public Err(Exception exception) : this()
    {
        var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
        errors = [new Diagnostic(exception, stackTrace.ToString())];
    }

    public Err(T value, ImmutableArray<Diagnostic> errors) : this()
    {
        this.value = value;
        this.errors = errors;
    }

    public Err(T value, Diagnostic error) : this(value, [error])
    {
    }

    public T Value => value;

    public ImmutableArray<Diagnostic> Errors => errors;

    public bool HasErrors => !errors.IsEmpty;


    public static implicit operator Err<T>(T value)
    {
        return new Err<T>(value);
    }

    public static implicit operator Err<T>(Diagnostic error)
    {
        return new Err<T>(error);
    }

    public static implicit operator Err<T>(ImmutableArray<Diagnostic> errors)
    {
        return new Err<T>(errors);
    }

    public static implicit operator Err<T>(Exception exception)
    {
        return new Err<T>(exception);
    }

    public static implicit operator Err<T>((T value, ImmutableArray<Diagnostic> errors) pair)
    {
        return new Err<T>(pair.value, pair.errors);
    }

    public static implicit operator Err<T>((T value, Diagnostic error) pair)
    {
        return new Err<T>(pair.value, pair.error);
    }
    
    public static implicit operator Err<T>((T value, Exception exception) pair)
    {
        return new Err<T>(pair.value, pair.exception);
    }

    // NB: This is explicit to force people to unwrap their errors properly.
    public static explicit operator T?(Err<T> err)
    {
        return err.HasErrors ? default : err.value;
    }

    public KafeErrorException AsException()
    {
        return new KafeErrorException(Errors);
    }

    public static T Unwrap(Err<T> err)
    {
        if (err.HasErrors)
        {
            throw err.AsException();
        }

        return err.Value;
    }

    public T Unwrap()
    {
        return Unwrap(this);
    }

    public T? GetValueOrDefault()
    {
        return HasErrors ? default : Value;
    }
}
