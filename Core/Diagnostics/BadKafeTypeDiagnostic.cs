namespace Kafe.Core.Diagnostics;

public record BadKafeTypeDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Moniker => "bad-type";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Type"),
        (Const.CzechCulture, "Neplatný typ")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid type."),
        (Const.CzechCulture, "Řetězec '{Value}' není platný typ.")
    );
}
