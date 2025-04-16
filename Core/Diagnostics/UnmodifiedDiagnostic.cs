using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

[DiagnosticPayload(Name = "unmodified")]
public record UnmodifiedDiagnostic(
    KafeType EntityType,
    Hrib Id
)
{
    public const DiagnosticSeverity DefaultSeverity = DiagnosticSeverity.Warning;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Unmodified"),
        (Const.CzechCulture, "Beze změny")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' was not modified."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' nebyl/a změněn/a.")
    );
}
