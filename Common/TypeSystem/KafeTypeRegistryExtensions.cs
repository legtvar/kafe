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
}
