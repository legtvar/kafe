using System;

namespace Kafe.Core.Diagnostics;

public record IncompatibleRequirementDiagnostic(
    Type RequirementType,
    Type ObjectType
) : IDiagnosticPayload
{
    public static string Moniker => "incompatible-requirement";

    public static DiagnosticSeverity Severity => DiagnosticSeverity.Error;

    public static LocalizedString Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Incompatible Requirement"),
        (Const.CzechCulture, "Nekompatibilní požadavek")
    );

    public static LocalizedString MessageFormat { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Requirement '{RequirementType}' does not support '{ObjectType}' objects."),
        (Const.CzechCulture, "Požadavek '{RequirementType}' nelze použít na objekty typu '{ObjectType}'.")
    );
}
