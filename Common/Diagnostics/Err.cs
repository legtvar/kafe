using System;
using System.Diagnostics.CodeAnalysis;

namespace Kafe;

/// <summary>
/// An error type. Can contain a <see cref="Diagnostic "/>, a value of <typeparamref name="T"/> or both.
/// </summary>
// Inspired in part by: https://stackoverflow.com/questions/3151702/discriminated-union-in-c-sharp
// And: https://ziglang.org/documentation/master/#while-with-Error-Unions
public readonly record struct Err<T>
{
    [MaybeNull]
    private readonly T value = default!;
    private readonly Diagnostic diagnostic = default;
    private readonly bool hasValue = false;
    private readonly bool hasDiagnostic = false;

    public Err()
    {
        value = default!;
        diagnostic = default;
    }

    public Err(T? value, Diagnostic? diagnostic)
    {
        if (value is null && diagnostic is null)
        {
            throw new ArgumentException("Err<T> must contain a non-null a value or a diagnostic or both.");
        }

        if (value is not null)
        {
            hasValue = true;
            this.value = value;
        }

        if (diagnostic is not null)
        {
            hasDiagnostic = true;
            this.diagnostic = diagnostic.Value;
        }
    }

    public Err(T value) : this(value, null)
    {
    }

    public Err(Diagnostic diagnostic) : this(default, diagnostic)
    {
    }

    [MaybeNull]
    public T Value => value;

    public Diagnostic Diagnostic => diagnostic;

    [MemberNotNullWhen(true, nameof(Diagnostic))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool HasDiagnostic => hasDiagnostic;

    [MemberNotNullWhen(true, nameof(Diagnostic))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool HasError => hasDiagnostic && diagnostic.Severity == DiagnosticSeverity.Error;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Diagnostic))]
    public bool HasValue => hasValue;

    public bool IsInvalid => !hasValue && !hasDiagnostic;


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

    public KafeErrorException AsException()
    {
        if (!HasError)
        {
            throw new InvalidOperationException("This Err<T> has no error diagnostic to convert into an exception.");
        }

        return new KafeErrorException(Diagnostic);
    }

    public static T Unwrap(Err<T> err)
    {
        if (err.HasError)
        {
            throw new KafeErrorException(err.Diagnostic);
        }

        return err.Value;
    }

    public T Unwrap()
    {
        return Unwrap(this);
    }

    [return: MaybeNull]
    public T GetValueOrDefault()
    {
        return hasValue ? value : default;
    }
}
