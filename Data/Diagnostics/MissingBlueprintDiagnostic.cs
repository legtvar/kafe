namespace Kafe.Data.Diagnostics;

public record MissingBlueprintDiagnostic(
    Hrib ProjectGroupId
) : IDiagnosticPayload
{
    public static string Moniker => "missing-blueprint";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Missing Blueprint"),
        (Const.CzechCulture, "Chybí blueprint")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Project group '{ProjectGroupId}' does not have a blueprint set."),
        (Const.CzechCulture, "Projektová skupina '{ProjectGroupId}' nemá nastavený blueprint.")
    );
}
