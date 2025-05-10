using System.Collections.Generic;

namespace Kafe;

/// <summary>
/// Prescription for the shard abstraction without specifics of the persistence layer.
/// </summary>
/// <remarks>
/// Shard representations in the persistence layer MUST implement this interface so that requirements can function
/// without reference to the specific persistence implementation.
/// </remarks>
public interface IShard : IEntity
{
    LocalizedString Name { get; }

    KafeObject Metadata { get; }

    long FileLength { get; }

    string MimeType { get; }

    IReadOnlySet<IShardLink> Links { get; }
}
