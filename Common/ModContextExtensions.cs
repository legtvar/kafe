namespace Kafe;

public static class ModContextExtensions
{
    public static ModContext AddType<T>(this ModContext c, KafeTypeUsage usage)
    {
        return c.AddType(typeof(T), usage);
    }
}
