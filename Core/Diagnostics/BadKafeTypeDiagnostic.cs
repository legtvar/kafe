using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

public record BadKafeTypeDiagnostic(
    string Value
) : IDiagnosticPayload
{
    public static string Name { get; } = "bad-type";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Bad Type"),
        (Const.CzechCulture, "Neplatný typ")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "String '{Value}' is not a valid type."),
        (Const.CzechCulture, "Řetězec '{Value}' není platný typ.")
    );
}
