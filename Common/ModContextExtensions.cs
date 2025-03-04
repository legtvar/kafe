namespace Kafe;

public static class ModContextExtensions
{
    public static ModContext AddType<T>(this ModContext c, ModContext.KafeTypeRegistrationOptions? options = null)
    {
        return c.AddType(typeof(T), options);
    }

    public static ModContext AddRequirement<T>(
        this ModContext c,
        ModContext.RequirementRegistrationOptions? options = null
    ) where T : IRequirement
    {
        return c.AddRequirement(typeof(T), options);
    }
}
