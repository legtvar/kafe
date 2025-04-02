using System;

namespace Kafe;

public class KafeErrorException : Exception
{
    public KafeErrorException(Diagnostic diagnostic)
        : base($"A '{diagnostic.Descriptor.KafeType}' has been thrown as exception.")
    {
        Diagnostic = diagnostic;
        StackTrace = diagnostic.StackTrace;
    }

    public Diagnostic Diagnostic { get; }

    public override string? StackTrace { get; }
}
