using System.Collections.Generic;

namespace Kafe;

/// <summary>
/// Prescription for the blueprint abstraction without specifics of the persistence layer.
/// </summary>
///
/// <remarks>
/// Blueprint representations in the persistence layer MUST implement this interface so that requirements can function
/// without reference to a specific persistence implementation.
/// </remarks>
public interface IBlueprint : IEntity
{
    static string IKafeTypeMetadata.Moniker => "blueprint";

    static LocalizedString IKafeTypeMetadata.Title => LocalizedString.Create(
        (Const.InvariantCulture, "Blueprint"),
        (Const.CzechCulture, "Nákres")
    );

    LocalizedString Name { get; }

    IReadOnlyDictionary<string, IBlueprintProperty> Properties { get; }

    bool AllowAdditionalProperties { get; }
}
