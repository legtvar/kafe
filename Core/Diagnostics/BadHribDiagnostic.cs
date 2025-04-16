using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

[DiagnosticPayload(Name = "bad-hrib")]
public record BadHribDiagnostic(
    string Value
)
{
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Bad HRIB"),
        (Const.CzechCulture, "Neplatný HRIB")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid identifier."),
        (Const.CzechCulture, "Řetězec '{Value}' není platný identifikátor.")
    );
}
