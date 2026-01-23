namespace Kafe;

public static class DiagnosticExtensions
{
    extension(Diagnostic diagnostic)
    {
        public Diagnostic ForParameter(string parameterName, int skipFrames = 1)
        {
            return new Diagnostic(
                payload: new ParameterDiagnostic(
                    Parameter: parameterName,
                    Inner: diagnostic
                ),
                severityOverride: diagnostic.Severity,
                skipFrames: skipFrames + 1
            );
        }
    }
}
