namespace Kafe.Core.Diagnostics;

public record EmptyHribDiagnostic
{
    public const string DiagnosticId = "empty-hrib";
    
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Empty HRIB"),
        (Const.CzechCulture, "Prázdný HRIB")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "This identifier must not be empty."),
        (Const.CzechCulture, "Tento identifikátor nesmí být prázdný.")
    );
}
