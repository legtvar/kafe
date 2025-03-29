namespace Kafe;

public static class KafeObjectFactoryExtensions
{
    public static KafeObject Wrap<T>(this KafeObjectFactory f, T value)
        where T : notnull
    {
        return f.Wrap(typeof(T), value);
    }
    
    public static KafeObject Wrap(this KafeObjectFactory f, object value)
    {
        return f.Wrap(value.GetType(), value);
    }
}

