namespace Kafe.Core.Diagnostics;

public record NotFoundDiagnostic(
    KafeType EntityType,
    Hrib Id
)
{
    public const string DiagnosticId = "not-found";

    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Not Found"),
        (Const.CzechCulture, "Nenalezeno")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' could not be found. Are you sure it exists?"),
        (Const.CzechCulture, "{EntityType:h} '{Id}' se nepodařilo nalézt. Jste si jistí, že existuje?")
    );
}
