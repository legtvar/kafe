using System;

namespace Kafe.Diagnostics;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DiagnosticPayloadAttribute : Attribute
{
    public string? Name { get; set; }
}
