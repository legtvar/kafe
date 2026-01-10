namespace Kafe.Core.Diagnostics;

public record EmptyHribDiagnostic : IDiagnosticPayload
{
    public static string Moniker => "empty-hrib";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Empty HRIB"),
        (Const.CzechCulture, "Prázdný HRIB")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "This identifier must not be empty."),
        (Const.CzechCulture, "Tento identifikátor nesmí být prázdný.")
    );
}
