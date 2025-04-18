namespace Kafe.Core.Diagnostics;

public record ParameterDiagnostic(
    string Parameter,
    Diagnostic Inner
) : IDiagnosticPayload
{
    public static string Name {get;} = "parameter";
    
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Parameter"),
        (Const.CzechCulture, "Parametr")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "Parameter '{Parameter}' has diagnostics."),
        (Const.CzechCulture, "K parametru '{Parameter}' jsou k dispozici hlášení.")
    );
}
