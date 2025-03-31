using System;
using System.Globalization;
using System.Text;

namespace Kafe;

public partial record struct KafeType : IFormattable
{
    /// <summary>
    /// The '{Mod}:{Primary}/{Secondary}[]' format.
    /// </summary>
    public const string UniversalFormat = "U";

    /// <summary>
    /// A localized description of the KafeType suitable for use in <see cref="Diagnostic"/>s.
    /// Does not include a leading indefinite article.
    /// </summary>
    public const string ShortHumanReadableFormat = "h";

    /// <summary>
    /// A localized description of the KafeType suitable for use in <see cref="Diagnostic"/>s.
    /// Includes a leading indefinite article.
    /// </summary>
    public const string LongHumanReadableFormat = "H";

    public static readonly LocalizedString ShortFallbackGeneralName = LocalizedString.Create(
        (Const.InvariantCulture, "object of type '{0}'"),
        (Const.CzechCulture, "objekt typu '{0}'")
    );

    public static readonly LocalizedString ShortFallbackShardName = LocalizedString.Create(
        (Const.InvariantCulture, "shard of type '{0}'"),
        (Const.CzechCulture, "střípek typu '{0}'")
    );

    public static readonly LocalizedString ShortFallbackDiagnosticName = LocalizedString.Create(
        (Const.InvariantCulture, "diagnostic of type '{0}'"),
        (Const.CzechCulture, "hlášení typu '{0}'")
    );

    public static readonly LocalizedString ShortFallbackRequirementName = LocalizedString.Create(
        (Const.InvariantCulture, "requirement of type '{0}'"),
        (Const.CzechCulture, "požadavek typu '{0}'")
    );

    public static readonly LocalizedString LongFallbackGeneralName = LocalizedString.Create(
        (Const.InvariantCulture, "an object of type '{0}'"),
        (Const.CzechCulture, "objekt typu '{0}'")
    );

    public static readonly LocalizedString LongFallbackShardName = LocalizedString.Create(
        (Const.InvariantCulture, "a shard of type '{0}'"),
        (Const.CzechCulture, "střípek typu '{0}'")
    );

    public static readonly LocalizedString LongFallbackDiagnosticName = LocalizedString.Create(
        (Const.InvariantCulture, "a diagnostic of type '{0}'"),
        (Const.CzechCulture, "hlášení typu '{0}'")
    );

    public static readonly LocalizedString LongFallbackRequirementName = LocalizedString.Create(
        (Const.InvariantCulture, "a requirement of type '{0}'"),
        (Const.CzechCulture, "požadavek typu '{0}'")
    );

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

    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        if (formatProvider is not CultureInfo culture)
        {
            throw new ArgumentException("The format provider must be a CultureInfo.", nameof(formatProvider));
        }

        culture ??= CultureInfo.InvariantCulture;
        format ??= UniversalFormat;
        switch (format)
        {
            case UniversalFormat:
                return ToString();
            case ShortHumanReadableFormat:
            case LongHumanReadableFormat:
                if (Name is not null)
                {
                    return Name[culture];
                }

                string? fallbackName;
                if (format == ShortHumanReadableFormat)
                {
                    fallbackName = Primary switch
                    {
                        ShardPrimary => ShortFallbackShardName[culture],
                        RequirementPrimary => ShortFallbackRequirementName[culture],
                        DiagnosticPrimary => ShortFallbackDiagnosticName[culture],
                        _ => ShortFallbackGeneralName[culture]
                    };
                }
                else
                {
                    fallbackName = Primary switch
                    {
                        ShardPrimary => LongFallbackShardName[culture],
                        RequirementPrimary => LongFallbackRequirementName[culture],
                        DiagnosticPrimary => LongFallbackDiagnosticName[culture],
                        _ => LongFallbackGeneralName[culture]
                    };
                }

                return string.Format(fallbackName, ToString());
            default:
                throw new ArgumentException($"Format '{format}' is not recognized.", nameof(format));
        }
    }
}
