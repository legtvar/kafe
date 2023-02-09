using System;
using System.Text;

namespace Kafe.Data;

/// <summary>
/// Human-Readable Identifier Ballast
/// </summary>
public record Hrib
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    public const int Length = 11;

    public const string Invalid = "Invalid HRIB";

    private static readonly Random Random = new Random();

    private Hrib(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value));
        }
        Value = value;
    }

    public string Value { get; init; }

    public static implicit operator string(Hrib hrib)
    {
        return hrib.Value;
    }

    public static implicit operator Hrib(string value)
    {
        return new(value);
    }

    public static Hrib Create()
    {
        var sb = new StringBuilder(Length);
        for (int i = 0; i < Length; i++)
        {
            sb.Append(Alphabet[Random.Next(Alphabet.Length)]);
        }
        return new Hrib(sb.ToString());
    }

    public override string ToString()
    {
        return Value;
    }
}
