using System;

namespace Kafe;

public class KafeErrorException : Exception
{
    public KafeErrorException(Diagnostic diagnostic)
        : base($"A diagnostic with a '{diagnostic.Payload.GetType().Name}' payload has been thrown as exception.")
    {
        Diagnostic = diagnostic;
        StackTrace = diagnostic.StackTrace;
    }

    public Diagnostic Diagnostic { get; }

    public override string? StackTrace { get; }
}
