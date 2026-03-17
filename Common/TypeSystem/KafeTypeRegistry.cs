using System;
using System.Collections.Generic;

namespace Kafe;

public class KafeTypeRegistry : IFreezable
{
    private readonly Dictionary<KafeType, KafeTypeMetadata> types = new();

    private readonly Dictionary<Type, KafeType> dotnetTypeMap = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, KafeTypeMetadata> Types { get; }

    public IReadOnlyDictionary<Type, KafeType> DotnetTypeMap { get; }

    public KafeTypeRegistry()
    {
        Types = types.AsReadOnly();
        DotnetTypeMap = dotnetTypeMap.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public void Register(KafeTypeMetadata metadata)
    {
        AssertUnfrozen();
        if (!metadata.KafeType.IsValid)
        {
            throw new ArgumentException("Only valid KAFE types can be registered.");
        }

        if (!types.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException(
                $"KafeType '{metadata.KafeType}' has been already registered.",
                nameof(metadata)
            );
        }

        if (metadata.KafeType.DotnetType is null)
        {
            metadata = metadata with
            {
                KafeType = metadata.KafeType with { DotnetType = metadata.DotnetType }
            };
        }

        if (metadata.KafeType.DotnetType != metadata.DotnetType)
        {
            throw new ArgumentException(
                $"{nameof(KafeType)}.{nameof(KafeType.DotnetType)} must be null or "
                + $"match the {nameof(metadata.DotnetType)} in the metadata.",
                nameof(metadata)
            );
        }

        if (!dotnetTypeMap.TryAdd(metadata.DotnetType, metadata.KafeType))
        {
            var existing = dotnetTypeMap[metadata.DotnetType];
            throw new ArgumentException(
                $"KafeType '{metadata.KafeType}' cannot be mapped to .NET type '{metadata.DotnetType.FullName}' "
                + $"because that .NET type has already been registered by '{existing}'.",
                nameof(metadata)
            );
        }

        foreach (var alias in metadata.DotnetTypeAliases)
        {
            if (!dotnetTypeMap.TryAdd(alias, metadata.KafeType))
            {
                var existing = dotnetTypeMap[alias];
                throw new ArgumentException(
                    $"Cannot assign '{alias}' as an alias to '{metadata.KafeType}' because that .NET type has already "
                    + $"been registered by '{existing}'.",
                    nameof(metadata)
                );
            }
        }
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException(
                $"This {nameof(KafeTypeRegistry)} is frozen and can no longer be modified."
            );
        }
    }
}
