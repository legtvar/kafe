namespace Kafe.Core.Diagnostics;

public record AlreadyExistsDiagnostic(
    KafeType EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Moniker => "already-exists";
    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Entity Already Exists"),
        (Const.CzechCulture, "Entita již existuje")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:H} with identifier '{Id}' already exists."),
        (Const.CzechCulture, "{EntityType:H} s identifikátorem '{Id}' již existuje.")
    );
}
