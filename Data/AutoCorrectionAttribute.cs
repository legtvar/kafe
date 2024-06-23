using System;
using System.Globalization;

namespace Kafe.Data;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AutoCorrectionAttribute : Attribute
{
    public AutoCorrectionAttribute(string implementedOn)
    {
        ImplementedOn = DateTimeOffset.Parse(
            implementedOn,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);
    }
    public DateTimeOffset ImplementedOn { get; }
}
