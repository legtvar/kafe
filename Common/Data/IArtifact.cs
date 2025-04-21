using System.Collections.Generic;

namespace Kafe;

/// <summary>
/// Prescription for the artifact abstraction without specifics of the persistence layer.
/// </summary>
/// 
/// <remarks>
/// Artifact representations in the persistence layer MUST implement this interface so that requirements can function
/// without reference to a specific persistence implementation.
/// </remarks>
public interface IArtifact : IEntity
{
    LocalizedString Name { get; }

    IReadOnlyDictionary<string, KafeObject> Properties { get; }
}
