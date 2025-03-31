namespace Kafe.Core.Diagnostics;

public record LockedDiagnostic(
    KafeType EntityType,
    Hrib Id
)
{
    public const string DiagnosticId = "locked";
    
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Locked"),
        (Const.CzechCulture, "Zamčeno")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' cannot be modified because it is locked."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' nelze modifikovat, jelikož je zamknutý/á.")
    );
}
