using System;

namespace Kafe;

public static class KafeTypeRegistryExtensions
{
    public static KafeType RequireType(this KafeTypeRegistry registry, Type type)
    {
        if (!registry.DotnetTypeMap.TryGetValue(type, out var kafeType))
        {
            throw new ArgumentException(
                $"Type '{type}' has no mapped KAFE type. Make sure it is registered.",
                nameof(type)
            );
        }

        return kafeType;
    }

    public static KafeType RequireType<T>(this KafeTypeRegistry registry)
    {
        return registry.RequireType(typeof(T));
    }

    public static KafeTypeMetadata RequireMetadata(this KafeTypeRegistry registry, KafeType type)
    {
        if (!registry.Types.TryGetValue(type, out var metadata))
        {
            throw new ArgumentException(
                $"KAFE type '{type}' is not registered, yet it is required.",
                nameof(type)
            );
        }

        return metadata;
    }

    public static (KafeTypeMetadata typeMetadata, TSubtypeMetadata subtypeMetadata) RequireMetadata<TSubtypeMetadata>(
        this KafeTypeRegistry registry,
        KafeType type
    )
        where TSubtypeMetadata : class, ISubtypeMetadata
    {
        var subtypeRegistry = registry.RequireSubtypeRegistry<TSubtypeMetadata>();
        var typeMetadata = registry.RequireMetadata(type);
        var subtypeMetadata = subtypeRegistry.RequireMetadata(type);
        return (typeMetadata, subtypeMetadata);
    }
    
    public static (KafeTypeMetadata typeMetadata, TSubtypeMetadata subtypeMetadata) RequireMetadata<TSubtypeMetadata>(
        this KafeTypeRegistry registry,
        Type type
    )
        where TSubtypeMetadata : class, ISubtypeMetadata
    {
        var kafeType = registry.RequireType(type);
        return registry.RequireMetadata<TSubtypeMetadata>(kafeType);
    }

    public static ISubtypeRegistry<TMetadata> RequireSubtypeRegistry<TMetadata>(this KafeTypeRegistry r)
        where TMetadata : class, ISubtypeMetadata
    {
        if (!r.SubtypeRegistries.TryGetValue(typeof(TMetadata), out var subtypeRegistry))
        {
            throw new InvalidOperationException($"No {nameof(ISubtypeRegistry)} has been registered "
                + $"for the '{typeof(TMetadata)}' metadata type. Are you missing a call to "
                + $"{nameof(KafeBrewingOptions)}.{nameof(KafeBrewingOptions.AddSubtypeRegistry)}?");
        }

        if (subtypeRegistry is not ISubtypeRegistry<TMetadata> typedSubtypeRegistry)
        {
            throw new InvalidOperationException($"The {nameof(ISubtypeRegistry)} for the "
                + $"'{typeof(TMetadata)}' metadata type does not implement {typeof(ISubtypeRegistry<TMetadata>)}.");
        }

        return typedSubtypeRegistry;
    }
}
