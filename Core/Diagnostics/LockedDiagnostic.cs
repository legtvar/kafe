namespace Kafe.Core.Diagnostics;

public record LockedDiagnostic(
    KafeType EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Moniker => "locked";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Locked"),
        (Const.CzechCulture, "Zamčeno")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' cannot be modified because it is locked."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' nelze modifikovat, jelikož je zamknutý/á.")
    );
}
