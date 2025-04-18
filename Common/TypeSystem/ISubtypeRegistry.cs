using System.Collections.Generic;

namespace Kafe;

/// <summary>
/// Essentially a marker type for <see cref="ISubtypeRegistry{TMetadata}"/>. You should implement that one instead.
/// </summary>
public interface ISubtypeRegistry : IFreezable
{
}

/// <summary>
/// Keeps metadata for a specific category of <see cref="KafeType"/>s.
/// </summary>
public interface ISubtypeRegistry<TMetadata> : ISubtypeRegistry
    where TMetadata : class, ISubtypeMetadata
{
    public IReadOnlyDictionary<KafeType, TMetadata> Metadata { get; }

    public void Register(TMetadata metadata);
}
