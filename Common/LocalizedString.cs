using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kafe;

[JsonConverter(typeof(LocalizedStringJsonConverter))]
public sealed partial class LocalizedString : IEquatable<LocalizedString>
{
    public static readonly LocalizedString Empty = new LocalizedString(
        ImmutableDictionary.CreateRange(
            [
                new KeyValuePair<string, string>(Const.InvariantCultureCode, string.Empty)
            ]
        )
    );

    private readonly ImmutableDictionary<string, string> data;

    public string this[CultureInfo culture] => this[culture.TwoLetterISOLanguageName];

    public string this[string cultureCode]
        => ToString(cultureCode) ?? throw new NullReferenceException("This localized string is empty.");

    public string Invariant => this[Const.InvariantCultureCode];

    public ImmutableDictionary<string, string> GetRaw() => data;

    private LocalizedString(ImmutableDictionary<string, string> data)
    {
        if (data.Count == 0)
        {
            throw new ArgumentException("Data must contain at least one localized string.");
        }

        this.data = data;
    }

    public static LocalizedString Create(IDictionary<string, string> data)
    {
        return new(data.ToImmutableDictionary());
    }

    public static LocalizedString Create(IReadOnlyDictionary<string, string> data)
    {
        return new(data.ToImmutableDictionary());
    }

    public static LocalizedString Create(IReadOnlyDictionary<CultureInfo, string> data)
    {
        return new(data.ToImmutableDictionary(c => c.Key.TwoLetterISOLanguageName, c => c.Value));
    }

    public static LocalizedString Create(params (CultureInfo localCulture, string localString)[] strings)
    {
        var builder = ImmutableDictionary.CreateBuilder<CultureInfo, string>();
        foreach (var pair in strings)
        {
            builder.Add(pair.localCulture, pair.localString);
        }

        return Create(builder.ToImmutable());
    }

    public static LocalizedString Create(string invariantString, CultureInfo localCulture, string localString)
    {
        return Create((CultureInfo.InvariantCulture, invariantString), (localCulture, localString));
    }

    public static LocalizedString CreateInvariant(string invariantString)
    {
        return Create((CultureInfo.InvariantCulture, invariantString));
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] LocalizedString? value)
    {
        return value is null || value.data.Values.All(string.IsNullOrEmpty);
    }

    public static bool IsTooLong(LocalizedString? value, int maxLength = 8 << 10)
    {
        return value is not null && value.Values.Any(v => v.Length > maxLength);
    }

    public static bool IsNullEmptyOrLong([NotNullWhen(false)] LocalizedString? value, int maxLength = 8 << 10)
    {
        return IsNullOrEmpty(value) || IsTooLong(value, maxLength);
    }

    public static LocalizedString Format(LocalizedString value, params IEnumerable<object?> args)
    {
        return new LocalizedString(
            value.data.ToImmutableDictionary(
                p => p.Key,
                p =>
                {
                    var arr = args.Select(a => a is LocalizedString lsa ? lsa[p.Key] : a).ToArray();
                    return string.Format(p.Value, arr);
                }
            )
        );
    }

    [return: NotNullIfNotNull(nameof(localized))]
    public static implicit operator ImmutableDictionary<string, string>?(LocalizedString? localized)
    {
        if (localized is null)
        {
            return null;
        }

        return localized.data;
    }

    [return: NotNullIfNotNull(nameof(data))]
    public static implicit operator LocalizedString?(ImmutableDictionary<string, string>? data)
    {
        if (data is null)
        {
            return null;
        }

        return new(data);
    }

    [return: NotNullIfNotNull(nameof(localized))]
    public static explicit operator string?(LocalizedString? localized)
    {
        return localized?[CultureInfo.InvariantCulture];
    }

    [return: NotNullIfNotNull(nameof(invariantString))]
    public static explicit operator LocalizedString?(string? invariantString)
    {
        if (invariantString is null)
        {
            return null;
        }

        return CreateInvariant(invariantString);
    }

    public static bool operator ==(LocalizedString? lhs, LocalizedString? rhs)
    {
        if (lhs is null && rhs is null)
        {
            return true;
        }

        return EqualityComparer<LocalizedString>.Default.Equals(lhs, rhs);
    }

    public static bool operator !=(LocalizedString? lhs, LocalizedString? rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator ==(LocalizedString? lhs, IDictionary<string, string>? rhs)
    {
        if (lhs is null && rhs is null)
        {
            return true;
        }

        if (rhs is null)
        {
            return false;
        }

        var rhsLocalizedString = Create(rhs);
        return EqualityComparer<LocalizedString>.Default.Equals(lhs, rhsLocalizedString);
    }

    public static bool operator !=(LocalizedString? lhs, IDictionary<string, string>? rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LocalizedString other)
        {
            return false;
        }

        return Equals(other);
    }

    public bool Equals(LocalizedString? other)
    {
        if (other is null || data.Count != other.data.Count)
        {
            return false;
        }

        foreach (var pair in data)
        {
            var otherValue = other.data.GetValueOrDefault(pair.Key);
            if (!EqualityComparer<string>.Default.Equals(pair.Value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return data.GetHashCode();
    }

    public override string? ToString()
    {
        return ToString(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
    }

    public string? ToString(CultureInfo culture)
    {
        return ToString(culture.TwoLetterISOLanguageName);
    }

    public string? ToString(string? cultureCode)
    {
        cultureCode ??= CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        return data.GetValueOrDefault(cultureCode)
            ?? data.GetValueOrDefault(Const.InvariantCultureCode)
            ?? data.GetValueOrDefault(Const.EnglishCultureName)
            ?? data.GetValueOrDefault(Const.CzechCultureName)
            ?? data.GetValueOrDefault(Const.SlovakCultureName)
            ?? data.Values.FirstOrDefault(); // return whatever is there
    }

    public bool HasVariant(string cultureCode)
    {
        if (cultureCode == Const.CzechOrSlovakPseudoCultureName)
        {
            return data.ContainsKey(Const.CzechCultureName) || data.ContainsKey(Const.SlovakCultureName);
        }

        return data.ContainsKey(cultureCode);
    }

    public bool HasVariant(CultureInfo culture)
    {
        return data.ContainsKey(culture.TwoLetterISOLanguageName);
    }

    public static LocalizedString Merge(LocalizedString? old, LocalizedString? @new)
    {
        var isOldEmpty = IsNullOrEmpty(old);
        var isNewEmpty = IsNullOrEmpty(@new);

        return !isOldEmpty && isNewEmpty ? old!
            : isOldEmpty && !isNewEmpty ? @new!
            : !isOldEmpty && !isNewEmpty ? old!.data.SetItems(@new!)
            : LocalizedString.Empty;
    }

    /// <summary>
    /// Generates value that can be used in <c>*InfoChanged</c> events.
    /// </summary>
    ///
    /// <remarks>
    /// Returns <c>null</c> if no change should be made or <paramref name="new"/>
    /// if it should override <paramref name="old"/>.
    /// </remarks>
    public static LocalizedString? MakeOverride(LocalizedString? old, LocalizedString? @new)
    {
        var isOldEmpty = IsNullOrEmpty(old);
        var isNewEmpty = IsNullOrEmpty(@new);

        if (old == @new)
        {
            return null;
        }

        return !isOldEmpty && isNewEmpty ? null
            : isOldEmpty && !isNewEmpty ? @new
            : !isOldEmpty && !isNewEmpty ? @new
            : null;
    }
}

public class LocalizedStringJsonConverter : JsonConverter<LocalizedString>
{
    public override LocalizedString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>?>(ref reader, options);
        if (dictionary is null)
        {
            return null;
        }

        return LocalizedString.Create((IDictionary<string, string>)dictionary);
    }

    public override void Write(Utf8JsonWriter writer, LocalizedString value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.GetRaw(), options);
    }
}
