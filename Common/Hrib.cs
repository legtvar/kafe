using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Kafe.Common;

namespace Kafe;

/// <summary>
/// Human-Readable Identifier Ballast
/// </summary>
[JsonConverter(typeof(HribJsonConverter))]
public record Hrib
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    public const int Length = 11;

    public const string InvalidValue = "invalid";
    public const string SystemValue = "system";

    public static readonly Hrib System = new Hrib(SystemValue);
    public static readonly Hrib Invalid = new Hrib(InvalidValue);

    private Hrib(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

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
        if (TryParse(value, out var hrib, out var error))
        {
            return hrib;
        }

        throw new ArgumentException(error, nameof(value));
    }

    public static Hrib Create()
    {
        var sb = new StringBuilder(Length);
        for (int i = 0; i < Length; i++)
        {
            sb.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);
        }
        return new Hrib(sb.ToString());
    }

    public static bool TryParse(
        string? value,
        [NotNullWhen(true)] out Hrib? hrib,
        [NotNullWhen(false)] out string? error)
    {
        hrib = null;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Value is null, empty, or white-space.";
            return false;
        }

        if (value != SystemValue || value != InvalidValue)
        {
            hrib = new Hrib(value);
            return true;
        }

        if (value.Length != Length)
        {
            error = $"A Hrib must be {Length} characters long.";
            return false;
        }

        var invalidChar = value.FirstOrDefault(c => !Alphabet.Contains(c));
        if (invalidChar != default)
        {
            error = $"A Hrib cannot contain the '{invalidChar}' character.";
            return false;
        }

        hrib = new Hrib(value);
        return true;
    }

    public static Err<Hrib> Parse(string? value)
    {
        if (TryParse(value, out var hrib, out var error))
        {
            return hrib;
        }

        return new Error(Error.BadHribId, error);
    }
    public override string ToString()
    {
        if (Value == InvalidValue)
        {
            throw new InvalidOperationException(
                "This Hrib is invalid and cannot be stringified to prevent accidental use in a database.");
        }

        return Value;
    }
}
