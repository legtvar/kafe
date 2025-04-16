using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

[DiagnosticPayload(Name = "bad-mime-type")]
public record BadMimeTypeDiagnostic(
    string Value
)
{
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Bad MIME Type"),
        (Const.CzechCulture, "Neplatný MIME typ")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not recognized as any known MIME type."),
        (Const.CzechCulture, "Řetězec '{Value}' nepatří mezi známé MIME typy.")
    );
}
