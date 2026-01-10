namespace Kafe.Core.Diagnostics;

public record BadMimeTypeDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Moniker => "bad-mime-type";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Bad MIME Type"),
        (Const.CzechCulture, "Neplatný MIME typ")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not recognized as any known MIME type."),
        (Const.CzechCulture, "Řetězec '{Value}' nepatří mezi známé MIME typy.")
    );
}
