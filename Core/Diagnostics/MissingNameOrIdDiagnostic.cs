namespace Kafe.Core.Diagnostics;

public record MissingNameOrIdDiagnostic(
    KafeType EntityType
) : IDiagnosticPayload
{
    public static string Name { get; } = "missing-name-or-id";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Name or Id"),
        (Const.CzechCulture, "Chybí jméno nebo idetifikátor")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} must have an ID, name, or both set."),
        (Const.CzechCulture, "{EntityType:h} musí mít nastaveno ID, jméno nebo obojí.")
    );
}
