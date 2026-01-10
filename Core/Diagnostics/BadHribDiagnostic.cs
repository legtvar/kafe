namespace Kafe.Core.Diagnostics;

public record BadHribDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Moniker => "bad-hrib";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Bad HRIB"),
        (Const.CzechCulture, "Neplatný HRIB")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid identifier."),
        (Const.CzechCulture, "Řetězec '{Value}' není platný identifikátor.")
    );
}
