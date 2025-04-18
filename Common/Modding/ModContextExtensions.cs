using System;

namespace Kafe;

public static class ModContextExtensions
{
    public static ISubtypeRegistry<TMetadata> RequireSubtypeRegistry<TMetadata>(this ModContext c)
        where TMetadata : class, ISubtypeMetadata
    {
        if (!c.SubtypeRegistries.TryGetValue(typeof(TMetadata), out var subtypeRegistry))
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
