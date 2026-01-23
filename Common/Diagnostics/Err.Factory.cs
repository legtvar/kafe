namespace Kafe;

public static class Err
{
    public static Err<T> Wrap<T>(
        T value,
        IDiagnosticPayload diagnosticPayload,
        DiagnosticSeverity? severityOverride = null,
        DiagnosticDescriptor? descriptorOverride = null,
        int skipFrames = 1
    )
    {
        return new Err<T>(
            value,
            new Diagnostic(
                diagnosticPayload,
                skipFrames: skipFrames + 1,
                descriptorOverride: descriptorOverride,
                severityOverride: severityOverride
            )
        );
    }

    public static Err<T> Fail<T>(
        IDiagnosticPayload diagnosticPayload,
        DiagnosticDescriptor? descriptorOverride = null
    )
    {
        return Wrap<T>(default!, diagnosticPayload, DiagnosticSeverity.Error, descriptorOverride, skipFrames: 2);
    }

    public static Diagnostic Fail(
        IDiagnosticPayload diagnosticPayload,
        DiagnosticDescriptor? descriptorOverride = null
    )
    {
        return Diagnostic.Fail(diagnosticPayload, descriptorOverride, skipFrames: 2);
    }

    public static Err<T> Warn<T>(
        T value,
        IDiagnosticPayload diagnosticPayload,
        DiagnosticDescriptor? descriptorOverride = null
    )
    {
        return Wrap<T>(value, diagnosticPayload, DiagnosticSeverity.Warning, descriptorOverride, skipFrames: 2);
    }
}
