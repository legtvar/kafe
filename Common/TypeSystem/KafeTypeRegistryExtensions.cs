using System;

namespace Kafe;

public static class KafeTypeRegistryExtensions
{
    extension(KafeTypeMetadata metadata)
    {
        public TExtension RequireExtension<TExtension>()
        {
            if (metadata.Extension is not TExtension extension)
            {
                throw new ArgumentException(
                    $"KAFE type '{metadata.KafeType}' is registered but does not possess the required metadata extension "
                    + "of type '{typeof(TExtension)}'."
                );
            }

            return extension;
        }
    }

    extension(KafeTypeRegistry registry)
    {
        public KafeType RequireType(Type type)
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

        public KafeType RequireType<T>()
        {
            return registry.RequireType(typeof(T));
        }

        public KafeTypeMetadata RequireMetadata(KafeType type)
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
}
