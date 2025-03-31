namespace Kafe.Core.Diagnostics;

public record AlreadyExistsDiagnostic(
    KafeType EntityType,
    Hrib Id
)
{
    public const string DiagnosticId = "already-exists";

    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Entity Already Exists"),
        (Const.CzechCulture, "Entita již existuje")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:H} with identifier '{Id}' already exists."),
        (Const.CzechCulture, "{EntityType:H} s identifikátorem '{Id}' již existuje.")
    );
}
