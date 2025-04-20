namespace Kafe.Core.Diagnostics;

public record BadHribDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "bad-hrib";
    
    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Bad HRIB"),
        (Const.CzechCulture, "Neplatný HRIB")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid identifier."),
        (Const.CzechCulture, "Řetězec '{Value}' není platný identifikátor.")
    );
}
