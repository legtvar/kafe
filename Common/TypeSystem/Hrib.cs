using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Kafe;

/// <summary>
/// Human-Readable Identifier Ballast
/// </summary>
[JsonConverter(typeof(HribJsonConverter))]
public record Hrib : IParsable<Hrib>, IInvalidable<Hrib>
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    public const int Length = 11;

    public const string InvalidValue = "invalid";
    public const string SystemValue = "system";
    public const string EmptyValue = "empty";

    /// <summary>
    /// Special HRIB representing a non-entity, the system itself.
    /// </summary>
    public static readonly Hrib System = new(SystemValue);

    /// <summary>
    /// Special HRIB representing an error state.
    /// </summary>
    public static readonly Hrib Invalid = new(InvalidValue);

    static Hrib IInvalidable<Hrib>.Invalid => Invalid;

    /// <summary>
    /// Special HRIB for newly-created entities that don't have an Id yet.
    /// </summary>
    public static readonly Hrib Empty = new(EmptyValue);

    private Hrib(string value)
    {
        RawValue = value;
    }

    /// <summary>
    /// The raw identifier string. Use only if you know what you're doing. Prefer <see cref="ToString()"/> instead.
    /// </summary>
    public string RawValue { get; init; }

    public bool IsEmpty => RawValue == EmptyValue;

    public bool IsInvalid => RawValue == InvalidValue;

    public bool IsValid => RawValue != InvalidValue;

    /// <summary>
    /// Checks whether this HRIB is a regular id or `system`.
    /// Returns false for <see cref="Invalid"/> and <see cref="Empty"/>.
    /// </summary>
    public bool IsValidNonEmpty => !IsInvalid && !IsEmpty;

    // NB: this conversion is deliberately left `explicit` to prevent type casting issues with Marten/Postgres
    [return: NotNullIfNotNull(nameof(hrib))]
    public static explicit operator string?(Hrib? hrib)
    {
        if (hrib is null)
        {
            return null;
        }

        return hrib.ToString();
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static implicit operator Hrib?(string? value)
    {
        if (value is null)
        {
            return null;
        }

        if (TryParse(value, out var hrib, out var error))
        {
            return hrib;
        }

        throw new ArgumentException($"Failed to parse '{value}' as a HRIB ({error})");
    }

    public static Hrib Create()
    {
        return new Hrib(RandomNumberGenerator.GetString(Alphabet, Length));
    }

    public static bool TryParse(
        string? value,
        [NotNullWhen(true)] out Hrib? hrib,
        out HribParsingError error
    )
    {
        hrib = null;
        error = HribParsingError.None;

        if (value is null)
        {
            // Value is null.
            error = HribParsingError.ValueIsNull;
            return false;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            // Value is an empty string, or made up of white-space.
            error = HribParsingError.ValueIsEmpty;
            return false;
        }

        if (value == SystemValue || value == InvalidValue || value == EmptyValue)
        {
            hrib = new Hrib(value);
            return true;
        }

        if (value.Length != Length)
        {
            // A Hrib must be {Length} characters long.
            error = HribParsingError.BadLength;
            return false;
        }

        var invalidChar = value.FirstOrDefault(c => !Alphabet.Contains(c));
        if (invalidChar != default)
        {
            // A Hrib cannot contain the '{invalidChar}' character.
            error = HribParsingError.BadCharacter;
            return false;
        }

        hrib = new Hrib(value);
        return true;
    }

    public static Hrib Parse(string value)
    {
        if (!TryParse(value, out var hrib, out var error))
        {
            throw new ArgumentException($"Could not parse '{value}' as HRIB: {error}");
        }
        return hrib;
    }

    static Hrib IParsable<Hrib>.Parse(string value, IFormatProvider? _)
    {
        return Parse(value);
    }

    static bool IParsable<Hrib>.TryParse(
        [NotNullWhen(true)] string? value,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Hrib hrib
    )
    {
        return TryParse(value, out hrib, out _);
    }

    /// <summary>
    /// Parses <paramref name="value"/> and ensures it's a valid, and optionally non-empty and non-system
    /// <see cref="Hrib"/>.
    ///
    /// </summary>
    /// <param name="value">Value to be parsed</param>
    /// <param name="shouldReplaceEmpty">Replace <see cref="Empty"/> with a fresh <see cref="Hrib"/></param>
    /// <param name="shouldDisallowSystem">Disallow parsing the system HRIB</param>
    /// <returns>A valid HRIB or an error</returns>
    public static bool TryParseValid(
        string value,
        [NotNullWhen(true)] out Hrib? hrib,
        out HribParsingError error,
        bool shouldReplaceEmpty = false,
        bool shouldDisallowSystem = true
    )
    {
        if (!TryParse(value, out hrib, out error))
        {
            return false;
        }

        if (hrib.IsInvalid)
        {
            error = HribParsingError.InvalidDisallowed;
            return false;
        }

        if (hrib.IsEmpty)
        {
            if (shouldReplaceEmpty)
            {
                hrib = Create();
                return true;
            }

            error = HribParsingError.EmptyDisallowed;
            return false;
        }

        if (hrib == System && shouldDisallowSystem)
        {
            error = HribParsingError.SystemDisallowed;
            return false;
        }

        return true;
    }

    public string ToString(bool throwOnInvalidAndEmpty)
    {
        if (throwOnInvalidAndEmpty && RawValue == InvalidValue)
        {
            throw new InvalidOperationException(
                "This Hrib is invalid and cannot be stringified to prevent accidental use in a database."
            );
        }

        if (throwOnInvalidAndEmpty && RawValue == EmptyValue)
        {
            throw new InvalidOperationException(
                "This Hrib is empty and cannot be stringified to prevent accidental use in a database."
                + "This value is meant to be replaced with a proper HRIB by an entity service."
            );
        }

        return RawValue;
    }

    public override string ToString()
    {
        return ToString(throwOnInvalidAndEmpty: true);
    }

    /// <summary>
    /// The possible results of a <see cref="Hrib"/> parsing operation.
    /// </summary>
    ///
    /// <remarks>
    /// If you're wondering why we don't just use a <see cref="Err{T}"/> here, it's because <see cref="Hrib"/> is
    /// outside of the <see cref="IMod"/> ecosystem and thus cannot reference any <see cref="DiagnosticDescriptor"/>s.
    /// It kind of sucks, but it's better to have access to our custom ID type wherever you need it.
    /// </remarks>
    public enum HribParsingError
    {
        None,
        ValueIsNull,
        ValueIsEmpty,
        BadLength,
        BadCharacter,
        InvalidDisallowed,
        EmptyDisallowed,
        SystemDisallowed
    }
}
