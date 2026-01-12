using System;

namespace Kafe.Core.Diagnostics;

public record UnmodifiedDiagnostic(
    Type EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Moniker => "unmodified";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Warning;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Unmodified"),
        (Const.CzechCulture, "Beze změny")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' was not modified."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' nebyl/a změněn/a.")
    );
}
