using System;
using System.Globalization;
using System.Text;

namespace Kafe;

public readonly partial record struct KafeType : IFormattable
{
    /// <summary>
    /// The '{Mod}:{Primary}/{Secondary}[]' format.
    /// </summary>
    public const string UniversalFormat = "U";

    /// <summary>
    /// A localized description of the KafeType suitable for use in <see cref="Diagnostic"/>s.
    /// Does not include a leading indefinite article.
    /// </summary>
    public const string HumanReadableFormat = "h";

    public static readonly LocalizedString FallbackName = LocalizedString.Create(
        (Const.InvariantCulture, "object of type '{0}'"),
        (Const.CzechCulture, "objekt typu '{0}'")
    );

    public override readonly string ToString()
    {
        var sb = new StringBuilder(Mod.Length + 1 + Category.Length + 1 + Moniker?.Length ?? 0 + 2);
        sb.Append(Mod);
        sb.Append(ModCategorySeparator);

        if (Moniker != null)
        {
            sb.Append(ModCategorySeparator);
            sb.Append(Category);
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
            case HumanReadableFormat:
                if (Name is not null)
                {
                    return Name[culture];
                }

                return string.Format(FallbackName[culture], ToString());
            default:
                throw new ArgumentException($"Format '{format}' is not recognized.", nameof(format));
        }
    }
}
