using System;
using System.Diagnostics.CodeAnalysis;

namespace Kafe;

/// <summary>
/// An error type. Can contain a <see cref="Diagnostic "/>, a value of <typeparamref name="T"/> or both.
/// </summary>
/// <remarks>
/// Guarantees that if <see cref="HasError"/> is false then <see cref="Value"/> is not null nor
/// <see cref="IInvalidable{T}.Invalid"/>.
/// Also, if <see cref="HasValue"/> is false then <see cref="HasError"/> must be true.
/// However, if <see cref="HasValue"/> is true, then <see cref="HasError"/> MAY still be true and there MAY
/// be an error <see cref="Diagnostic"/>.
/// In other words, <see cref="Value"/> may be null, but when it is, there must be an error <see cref="Diagnostic"/>
/// explaining why.
/// </remarks>
// Inspired in part by: https://stackoverflow.com/questions/3151702/discriminated-union-in-c-sharp
// And: https://ziglang.org/documentation/master/#while-with-Error-Unions
public readonly record struct Err<T>
{
    private readonly T? value = default;

    private readonly Diagnostic diagnostic = Diagnostic.Invalid;

    public Err()
    {
        value = default!;
        diagnostic = default;
    }

    public Err(T? value, Diagnostic diagnostic)
    {
        if (value is null && !diagnostic.IsValid)
        {
            throw new ArgumentException("Cannot create an Err<T> using an invalid diagnostic.", nameof(diagnostic));
        }

        if ((value is null || value is IInvalidable { IsValid: false })
            && (diagnostic.Severity != DiagnosticSeverity.Error))
        {
            throw new ArgumentException(
                "Value cannot be null or invalid if there is no error diagnostic.",
                nameof(value)
            );
        }

        this.value = value;
        this.diagnostic = diagnostic;
    }

    public Err(T value) : this(value, Diagnostic.Invalid)
    {
    }

    public Err(Diagnostic diagnostic) : this(default!, diagnostic)
    {
    }

    public T? Value => value;

    public Diagnostic Diagnostic => !HasValue && diagnostic is not { IsValid: true, Severity: DiagnosticSeverity.Error }
        ? new Diagnostic(new GenericErrorDiagnostic(), skipFrames: 2)
        : diagnostic;

    /// <summary>
    /// Is there an error diagnostic? If yes, <see cref="Value"/> is undefined.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    public bool HasError => Diagnostic is { IsValid: true, Severity: DiagnosticSeverity.Error };

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => value is not null
        && (value is not IInvalidable invalidableValue || invalidableValue.IsValid);


    public static implicit operator Err<T>(T value)
    {
        return new Err<T>(value);
    }

    public static implicit operator Err<T>(Diagnostic diagnostic)
    {
        return new Err<T>(diagnostic);
    }

    public static implicit operator Err<T>([DisallowNull] Diagnostic? diagnostic)
    {
        if (!diagnostic.HasValue)
        {
            throw new ArgumentException("Cannot turn a null diagnostic into an Err<T>.", nameof(diagnostic));
        }

        return new Err<T>(diagnostic.Value);
    }

    public static implicit operator Err<T>((T value, Diagnostic? diagnostic) pair)
    {
        return new Err<T>(pair.value, pair.diagnostic ?? Diagnostic.Invalid);
    }

    public static implicit operator Err<T>((T value, Diagnostic error) pair)
    {
        return new Err<T>(pair.value, pair.error);
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

    public T? GetValueOrDefault()
    {
        return HasValue ? Value : default;
    }
}
