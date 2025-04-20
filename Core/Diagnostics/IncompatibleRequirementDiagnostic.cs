namespace Kafe.Core.Diagnostics;

public record IncompatibleRequirementDiagnostic(
    KafeType RequirementType,
    KafeType ObjectType
) : IDiagnosticPayload
{
    public static string Moniker { get; } = "incompatible-requirement";

    public static DiagnosticSeverity DefaultSeverity { get; } = DiagnosticSeverity.Error;

    public static readonly LocalizedString Title = LocalizedString.Create(
        (Const.InvariantCulture, "Incompatible Requirement"),
        (Const.CzechCulture, "Nekompatibilní požadavek")
    );

    public static readonly LocalizedString MessageFormat = LocalizedString.Create(
        (Const.InvariantCulture, "Requirement '{RequirementType}' does not support '{ObjectType}' objects."),
        (Const.CzechCulture, "Požadavek '{RequirementType}' nelze použít na objekty typu '{ObjectType}'.")
    );
}
