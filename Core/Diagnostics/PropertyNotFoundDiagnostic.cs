namespace Kafe.Core.Diagnostics;

public record PropertyNotFoundDiagnostic(
    Hrib ArtifactId,
    string PropertyKey
) : IDiagnosticPayload
{
    public static string Moniker => "property-not-found";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Property Not Found"),
        (Const.CzechCulture, "Vlastnost nenalezena")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Artifact '{ArtifactId}' does not possess property '{PropertyKey}'."),
        (Const.CzechCulture, "Artefakt '{ArtifactId}' nemá vlastnost '{PropertyKey}'.")
    );
}
