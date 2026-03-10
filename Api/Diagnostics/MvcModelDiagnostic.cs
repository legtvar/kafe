namespace Kafe.Api.Diagnostics;

public record MvcModelDiagnostic(
    string Message
) : IDiagnosticPayload
{
    public static string Moniker => "bad-csv";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Model Diagnostic"),
        (Const.CzechCulture, "Chyba modelu")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{Message}")
    );
}
