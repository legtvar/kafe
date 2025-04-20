using System;
using System.Collections.Immutable;
using System.Text;

namespace Kafe;

public class FileLengthFormatter : IKafeFormatter
{
    /// <summary>
    /// Format the numbers using the International System of Units, using powers of 10
    /// (e.g., kilobyte (KB) = 1 000 B, megabyte (MB) = 1 000 000 B, etc.)
    /// </summary>
    public const string SIMetricSpecifier = "fs";

    /// <summary>
    /// Format the numbers using the system by the International Electrotechnical Commission, using powers of 2
    /// (e.g., kibibyte (KiB) = 1 024 B, mebibyte (MiB) = 1 048 576 B, etc.)
    /// </summary>
    public const string IECBinarySpecifier = "FS";

    public static readonly ImmutableArray<string> SIUnits = [
        "B",
        "kB", // Yes, in SI units the 'k' is small. MS and some others use JEDEC in place of IEC. Use IEC instead.
        "MB",
        "GB",
        "TB",
        "PB",
        "EB",
        "ZB",
        "YB",
        "RB",
        "QB"
    ];

    public static readonly ImmutableArray<string> IECUnits = [
        "B",
        "KiB",
        "MiB",
        "GiB",
        "TiB",
        "PiB",
        "EiB",
        "ZiB",
        "YiB",
        "RiB",
        "QiB"
    ];

    public bool CanFormat(string? format, Type argType)
    {
        return argType.IsInteger()
            && !string.IsNullOrWhiteSpace(format)
            && (format.StartsWith(SIMetricSpecifier) || format.StartsWith(IECBinarySpecifier));
    }

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        format ??= IECBinarySpecifier;
        arg ??= -1;
        var number = Convert.ToUInt64(arg);
        var scaledDown = 0.0;
        var sb = new StringBuilder();
        var unit = "B";
        if (format.StartsWith(SIMetricSpecifier))
        {
            var log = (int)(Math.Log10(number) / 3);
            log = Math.Clamp(log, 0, SIUnits.Length);
            unit = SIUnits[log];
            format = format[SIMetricSpecifier.Length..].TrimStart();
            scaledDown = number / Math.Pow(1000, log);
        }
        else if (format.StartsWith(IECBinarySpecifier))
        {
            var log = (int)(Math.Log2(number) / 10);
            log = Math.Clamp(log, 0, IECUnits.Length);
            unit = IECUnits[log];
            format = format[IECBinarySpecifier.Length..].TrimStart();
            scaledDown = number / Math.Pow(1024, log);
        }

        sb.Append(scaledDown.ToString(format, formatProvider));
        sb.Append(' ');
        sb.Append(unit);
        return sb.ToString();
    }
}
