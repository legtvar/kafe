using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

[DiagnosticPayload(Name = "bad-type")]
public record BadKafeTypeDiagnostic(
    string Value
)
{
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Type"),
        (Const.CzechCulture, "Neplatný typ")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid type."),
        (Const.CzechCulture, "Řetězec '{Value}' není platný typ.")
    );
}
