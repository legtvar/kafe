using System;

namespace Kafe;

public static class ISubtypeRegistryExtensions
{
    public static TMetadata RequireMetadata<TMetadata>(this ISubtypeRegistry<TMetadata> subtypeRegistry, KafeType type)
        where TMetadata : class, ISubtypeMetadata
    {
        if (!subtypeRegistry.Metadata.TryGetValue(type, out var metadata))
        {
            throw new ArgumentException(
                $"Type '{type}' is missing {typeof(TMetadata).Name}. "
                    + "Make sure it is registered in {subtypeRegistry.GetType()}.",
                nameof(type)
            );
        }

        return metadata;
    }
}
