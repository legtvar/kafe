namespace Kafe.Core.Diagnostics;

public record EmptyHribDiagnostic : IDiagnosticPayload
{
    public static string Name { get; } = "empty-hrib";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Empty HRIB"),
        (Const.CzechCulture, "Prázdný HRIB")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "This identifier must not be empty."),
        (Const.CzechCulture, "Tento identifikátor nesmí být prázdný.")
    );
}
