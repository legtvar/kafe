namespace Kafe.Core.Diagnostics;

public record RequiredDiagnostic : IDiagnosticPayload
{
    public static string Moniker => "required";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Required Value"),
        (Const.CzechCulture, "Povinná hodnota")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "This value must be provided as it is required."),
        (Const.CzechCulture, "Tato hodnota musí být vyplněna, jelikož je povinná.")
    );
}
