using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Kafe;

[JsonConverter(typeof(KafeTypeJsonConverter))]
public partial record struct KafeType : IParsable<KafeType>
{
    public const char ModPrimarySeparator = ':';
    public const char PrimarySecondarySeparator = '/';
    public const string ArraySuffix = "[]";

    public static readonly Regex Regex = GetRegex();

    [GeneratedRegex($@"^(?<{nameof(Mod)}>[\w-]+):(?<{nameof(Primary)}>[\w-]+)"
        + $@"(?:\/(?<{nameof(Secondary)}>[\w-]+))?(?<{nameof(IsArray)}>\[\])?$")]
    private static partial Regex GetRegex();

    public static readonly KafeType Invalid = new("invalid", "invalid", default, default);

    public KafeType(
        string mod,
        string primary,
        string? secondary = null,
        bool isArray = false,
        LocalizedString? name = null
    )
    {
        Mod = mod;
        Primary = primary;
        Secondary = secondary;
        IsArray = isArray;
        Name = name;
    }

    public string Mod { get; }
    public string Primary { get; }
    public string? Secondary { get; }
    public bool IsArray { get; }

    /// <summary>
    /// A human-readable name.
    /// Used in <see cref="ToString(string?, IFormatProvider?)"/> with <see cref="HumanReadableFormat"/>.
    /// </summary>
    public LocalizedString? Name { get; init; }

    public readonly bool IsDefault => Mod == null && Primary == null && Secondary == null && IsArray == false;

    public readonly bool IsInvalid => this == Invalid;

    public readonly KafeType GetElementType()
    {
        if (!IsArray)
        {
            throw new NotSupportedException("Only array KAFE types may have element type.");
        }

        return new(Mod, Primary, Secondary, false);
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        [MaybeNullWhen(false)] out KafeType kafeType
    )
    {
        if (string.IsNullOrEmpty(s))
        {
            kafeType = Invalid;
            return false;
        }

        var match = Regex.Match(s);
        if (!match.Success)
        {
            kafeType = Invalid;
            return false;
        }

        kafeType = new(
            mod: match.Groups[nameof(Mod)].Value,
            primary: match.Groups[nameof(Primary)].Value,
            secondary: match.Groups[nameof(Secondary)].Success ? match.Groups[nameof(Secondary)].Value : null,
            isArray: match.Groups[nameof(IsArray)].Success
        );
        return true;
    }

    public static KafeType Parse(
        string s,
        IFormatProvider? provider
    )
    {
        if (!TryParse(s, out var kafeType))
        {
            throw new ArgumentException($"Could not parse '{s}' as a KafeType.", nameof(s));
        }
        return kafeType;
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out KafeType result
    )
    {
        return TryParse(s, out result);
    }
}
