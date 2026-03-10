namespace Kafe;

public static class DiagnosticExtensions
{
    extension(Diagnostic diagnostic)
    {
        public Diagnostic ForParameter(string parameterName, int skipFrames = 1)
        {
            return new Diagnostic(
                payload: new ParameterDiagnostic(
                    ParameterName: parameterName,
                    // TODO: Use json-everything's implementation of JSON pointers.
                    ParameterPointer: null!,
                    Inner: diagnostic
                ),
                severityOverride: diagnostic.Severity,
                skipFrames: skipFrames + 1
            );
        }
    }
}
