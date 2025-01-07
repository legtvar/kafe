using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
        string? secondary,
        bool isArray
    )
    {
        Mod = mod;
        Primary = primary;
        Secondary = secondary;
        IsArray = isArray;
    }

    public string Mod { get; }
    public string Primary { get; }
    public string? Secondary { get; }
    public bool IsArray { get; }

    public KafeType GetElementType()
    {
        if (!IsArray)
        {
            throw new NotSupportedException("Only array KAFE types may have element type.");
        }

        return new(Mod, Primary, Secondary, false);
    }

    public override readonly string ToString()
    {
        var sb = new StringBuilder(Mod.Length + 1 + Primary.Length + 1 + Secondary?.Length ?? 0 + 2);
        sb.Append(Mod);
        sb.Append(ModPrimarySeparator);

        if (Secondary != null)
        {
            sb.Append(ModPrimarySeparator);
            sb.Append(Primary);
        }

        if (IsArray)
        {
            sb.Append(ArraySuffix);
        }

        return sb.ToString();
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

    public static Err<KafeType> Parse(string? s)
    {
        if (!TryParse(s, out var kafeType))
        {
            return Error.BadKafeType(s);
        }

        return kafeType;
    }

    public static KafeType Parse(
        string s,
        IFormatProvider? provider
    )
    {
        return Parse(s).Unwrap();
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
