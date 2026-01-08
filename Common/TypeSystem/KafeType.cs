using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Kafe;

[JsonConverter(typeof(KafeTypeJsonConverter))]
public readonly partial record struct KafeType(
    string Mod,
    string Category,
    string Moniker,
    bool IsArray = false,
    LocalizedString? Name = null
) : IParsable<KafeType>, IInvalidable<KafeType>
{
    public const char ModCategorySeparator = ':';
    public const char CategoryMonikerSeparator = '/';
    public const string ArraySuffix = "[]";

    public static readonly Regex Regex = GetRegex();

    [GeneratedRegex(
        $@"^(?<{nameof(Mod)}>[\w-]+):(?<{nameof(Category)}>[\w-]+)"
        + $@"\/(?<{nameof(Moniker)}>[\w-]+)(?<{nameof(IsArray)}>\[\])?$"
    )]
    private static partial Regex GetRegex();

    public static KafeType Invalid { get; } = new(Const.InvalidId, Const.InvalidId, Const.InvalidId);

    public bool IsValid => this != Invalid;

    public string Mod { get; } = Mod;
    public string Category { get; } = Category;
    public string Moniker { get; } = Moniker;
    public bool IsArray { get; } = IsArray;

    /// <summary>
    /// A human-readable name.
    /// Used in <see cref="ToString(string?, IFormatProvider?)"/> with <see cref="HumanReadableFormat"/>.
    /// </summary>
    public LocalizedString? Name { get; init; } = Name;

    public bool IsDefault => Mod == null && Category == null && Moniker == null && IsArray == false;

    public KafeType GetElementType()
    {
        if (!IsArray)
        {
            throw new NotSupportedException("Only array KAFE types may have element type.");
        }

        return new KafeType(Mod, Category, Moniker, false);
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

        kafeType = new KafeType(
            Mod: match.Groups[nameof(Mod)].Value,
            Category: match.Groups[nameof(Category)].Value,
            Moniker: match.Groups[nameof(Moniker)].Value,
            IsArray: match.Groups[nameof(IsArray)].Success
        );
        return true;
    }

    public static KafeType Parse(
        string s,
        IFormatProvider? provider = null
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
