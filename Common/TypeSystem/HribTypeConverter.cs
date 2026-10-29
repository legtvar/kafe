using System;
using System.ComponentModel;
using System.Globalization;

namespace Kafe;

public class HribTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string strValue)
        {
            throw new InvalidOperationException("Can only convert strings to Hribs.");
        }

        return Hrib.Parse(strValue);
    }
}
