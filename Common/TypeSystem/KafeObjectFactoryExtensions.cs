using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

    public static ImmutableArray<KafeObject> WrapMany(this KafeObjectFactory f, params IEnumerable<object> values)
    {
        return [.. values.Select(f.Wrap)];
    }
}

