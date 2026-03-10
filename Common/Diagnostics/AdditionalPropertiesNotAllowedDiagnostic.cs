using System.Collections.Immutable;

namespace Kafe;

public record AdditionalPropertiesNotAllowedDiagnostic(
    Hrib ArtifactId,
    ImmutableArray<string> PropertyNames
) : IDiagnosticPayload
{
    public static string Moniker => "additional-properties-not-allowed";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Additional Properties Not Allowed"),
        (Const.CzechCulture, "Dodatečné vlastnosti nejsou povoleny")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture,
            "Artifact '{ArtifactId}' it not allowed to contain additional properties, which are not defined "
            + "in its blueprint, yet it does. These properties should not be set: {PropertyNames}."
        ),
        (Const.CzechCulture, "Artefakt '{ArtifactId}' nesmí obsahovat dodatečné vlastnosti, které nejsou dané "
            + "jeho blueprintem. Tyto vlastnosti by měly být odstraněny: {PropertyNames}."
        )
    );
}
