namespace Kafe;

public record ParameterDiagnostic(
    string Parameter,
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
        (Const.InvariantCulture, "Parameter '{Parameter}' has diagnostics."),
        (Const.CzechCulture, "K parametru '{Parameter}' jsou k dispozici hlášení.")
    );
}
