using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Kafe;

public class BitrateFormatter : IKafeFormatter
{
    public const string Specifier = "bps";

    public static readonly ImmutableArray<string> Units = [
        "bps",
        "kbps",
        "Mbps",
        "Gbps",
        "Tbps",
        "Pbps",
        "Ebps",
        "Zbps",
        "Ybps",
        "Rbps",
        "Qbps"
    ];

    public bool CanFormat(string? format, Type argType)
    {
        return argType.IsInteger()
            && !string.IsNullOrWhiteSpace(format)
            && format.StartsWith(Specifier);
    }

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        format ??= Specifier;
        arg ??= -1;
        var number = Convert.ToUInt64(arg);
        var scaledDown = 0.0;
        var sb = new StringBuilder();
        var unit = Units.First();
        if (format.StartsWith(Specifier))
        {
            var log = (int)(Math.Log10(number) / 3);
            log = Math.Clamp(log, 0, Units.Length);
            unit = Units[log];
            format = format[Units.Length..].TrimStart();
            scaledDown = number / Math.Pow(1000, log);
        }

        sb.Append(scaledDown.ToString(format, formatProvider));
        sb.Append(' ');
        sb.Append(unit);
        return sb.ToString();
    }
}
