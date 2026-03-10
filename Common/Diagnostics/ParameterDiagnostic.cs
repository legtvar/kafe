namespace Kafe;

public record ParameterDiagnostic(
    string ParameterName,
    string ParameterPointer,
    Diagnostic Inner
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "parameter";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Parameter"),
        (Const.CzechCulture, "Parametr")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Parameter '{ParameterName}' has diagnostics."),
        (Const.CzechCulture, "K parametru '{ParameterName}' jsou k dispozici hlášení.")
    );
}
