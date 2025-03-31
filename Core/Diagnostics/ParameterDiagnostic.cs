namespace Kafe.Core.Diagnostics;

public record ParameterDiagnostic(
    string Parameter,
    Diagnostic Inner
)
{
    public const string DiagnosticId = "parameter";

    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Parameter"),
        (Const.CzechCulture, "Parametr")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "Parameter '{Parameter}' has diagnostics."),
        (Const.CzechCulture, "K parametru '{Parameter}' jsou k dispozici hlášení.")
    );
}
