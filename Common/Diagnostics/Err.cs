using System;
using System.Diagnostics.CodeAnalysis;

namespace Kafe;

/// <summary>
/// An error union type. Can be used to return exceptions instead of throwing them.
/// </summary>
public readonly record struct Err<T>
{
    // Inspired in part by: https://stackoverflow.com/questions/3151702/discriminated-union-in-c-sharp
    // And: https://ziglang.org/documentation/master/#while-with-Error-Unions

    private readonly T value;
    private readonly Diagnostic? diagnostic;

    public Err()
    {
        value = default!;
        diagnostic = null;
    }

    public Err(T value, Diagnostic? diagnostic)
    {
        this.value = value;
        this.diagnostic = diagnostic;
    }

    public Err(T value) : this(value, null)
    {
    }

    public Err(Diagnostic diagnostic) : this(default!, diagnostic)
    {
    }

    public T Value => value;

    public Diagnostic? Diagnostic => diagnostic;


    public static implicit operator Err<T>(T value)
    {
        return new Err<T>(value);
    }

    public static implicit operator Err<T>(Diagnostic error)
    {
        return new Err<T>(error);
    }

    public static implicit operator Err<T>([DisallowNull] Diagnostic? error)
    {
        if (!error.HasValue)
        {
            throw new ArgumentException("Cannot turn a null diagnostic into an Err<T>.", nameof(error));
        }

        return new Err<T>(error.Value);
    }

    public static implicit operator Err<T>((T value, Diagnostic? diagnostic) pair)
    {
        return new Err<T>(pair.value, pair.diagnostic);
    }

    // NB: This is explicit to force people to unwrap their errors properly.
    public static explicit operator T(Err<T> err)
    {
        return err.Unwrap();
    }

    public KafeErrorException? AsException()
    {
        if (Diagnostic.HasValue && Diagnostic.Value.Severity == DiagnosticSeverity.Error)
        {
            return new KafeErrorException(Diagnostic.Value);
        }

        return null;
    }

    public static T Unwrap(Err<T> err)
    {
        var ex = err.AsException();
        if (ex is not null)
        {
            throw ex;
        }

        return err.Value;
    }

    public T Unwrap()
    {
        return Unwrap(this);
    }

    public T? GetValueOrDefault()
    {
        return Diagnostic is not null ? default : Value;
    }
}
