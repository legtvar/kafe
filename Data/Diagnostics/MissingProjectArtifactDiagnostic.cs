namespace Kafe.Data.Diagnostics;

public record MissingProjectArtifactDiagnostic(
    Hrib ProjectId
) : IDiagnosticPayload
{
    public static string Moniker => "missing-project-artifact";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Project Artifact"),
        (Const.CzechCulture, "Projektový artefakt chybí")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Project '{ProjectId}' does not have its inner artifact set."),
        (Const.CzechCulture, "Projekt '{ProjectId}' nemá nastaven svůj interní artefakt.")
    );
}
