using System;
using System.Collections.Immutable;

namespace Kafe;

public class AggregateFormatProvider : IFormatProvider, ICustomFormatter
{
    public AggregateFormatProvider(
        IFormatProvider @base,
        ImmutableArray<IKafeFormatter> inner
    )
    {
        Base = @base;
        Inner = inner;
    }

    public IFormatProvider Base { get; }

    public ImmutableArray<IKafeFormatter> Inner { get; }

    public object? GetFormat(Type? formatType)
    {
        if (formatType == typeof(ICustomFormatter) && Inner.Length > 0)
        {
            return this;
        }

        return Base.GetFormat(formatType);
    }

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (arg is null)
        {
            return string.Format(formatProvider, format ?? string.Empty, arg);
        }

        var argType = arg.GetType();
        foreach (var innerFormatter in Inner)
        {
            if (innerFormatter.CanFormat(format, argType))
            {
                return innerFormatter.Format(format, arg, formatProvider);
            }
        }

        throw new InvalidOperationException("No custom formatters have been registered. "
            + "This method should not have benn called and thus is a bug.");
    }

}
