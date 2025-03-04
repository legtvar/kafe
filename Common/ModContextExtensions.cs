namespace Kafe;

public static class ModContextExtensions
{
    internal static KafeType AddType<T>(
        this ModContext c,
        ModContext.KafeTypeRegistrationOptions? options = null
    )
    {
        return c.AddType(typeof(T), options);
    }

    public static KafeType AddArtifactProperty<T>(
        this ModContext c,
        ModContext.ArtifactPropertyRegistrationOptions? options = null
    )
    {
        return c.AddArtifactProperty(typeof(T), options);
    }

    public static KafeType AddRequirement<T>(
        this ModContext c,
        ModContext.RequirementRegistrationOptions? options = null
    ) where T : IRequirement
    {
        return c.AddRequirement(typeof(T), options);
    }

    public static KafeType AddShard<T>(
        this ModContext c,
        ModContext.ShardRegistrationOptions? options = null
    )
    {
        return c.AddShard(typeof(T), options);
    }
}
