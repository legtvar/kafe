namespace Kafe.Api.Diagnostics;

public record BadCsvDiagnostic : IDiagnosticPayload
{
    public static string Moniker => "bad-csv";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Badly Formatted CSV"),
        (Const.CzechCulture, "Špatně formátované CSV")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "The provided CSV file is not in the expected format."),
        (Const.CzechCulture, "Poskytnutý CSV soubor nesplňuje očekávaný formát.")
    );
}
