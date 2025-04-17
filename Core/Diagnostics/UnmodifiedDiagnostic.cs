using System;
using Kafe.Diagnostics;

namespace Kafe.Core.Diagnostics;

public record UnmodifiedDiagnostic(
    KafeType EntityType,
    Hrib Id
) : IDiagnosticPayload
{
    public static string Name { get; } = "unmodified";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Warning;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Unmodified"),
        (Const.CzechCulture, "Beze změny")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "{EntityType:h} '{Id}' was not modified."),
        (Const.CzechCulture, "{EntityType:h} '{Id}' nebyl/a změněn/a.")
    );
}
