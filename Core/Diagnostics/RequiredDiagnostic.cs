namespace Kafe.Core.Diagnostics;

public record RequiredDiagnostic
{
    public const string DiagnosticId = "required";
    
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Required Value"),
        (Const.CzechCulture, "Povinná hodnota")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "This value must be provided as it is required."),
        (Const.CzechCulture, "Tato hodnota musí být vyplněna, jelikož je povinná.")
    );
}
