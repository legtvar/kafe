using System;

namespace Kafe;

public static class ModContextExtensions
{
    public static ISubtypeRegistry<TMetadata> RequireSubtypeRegistry<TMetadata>(this ModContext c)
        where TMetadata : class, ISubtypeMetadata
    {
        return c.TypeRegistry.RequireSubtypeRegistry<TMetadata>();
    }
}
