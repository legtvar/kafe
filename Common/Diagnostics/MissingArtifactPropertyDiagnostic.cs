namespace Kafe.Diagnostics;

public record MissingArtifactPropertyDiagnostic(
    string PropertyKey,
    Hrib ArtifactId,
    Hrib BlueprintId
) : IDiagnosticPayload
{
    public static string Moniker => "missing-artifact-property";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Artifact Property"),
        (Const.CzechCulture, "Chybí vlastnost artefaktu")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (
            Const.InvariantCulture,
            "Artifact '{ArtifactId}' is missing the required '{PropertyKey}' property "
            + "according to the '{BlueprintId}' blueprint."
        ),
        (
            Const.CzechCulture,
            "Artefakt '{ArtifactId}' neobsahuje požadovanou vlastnost '{PropertyKey}' "
            + "podle blueprintu '{BlueprintId}'."
        )
    );
}
