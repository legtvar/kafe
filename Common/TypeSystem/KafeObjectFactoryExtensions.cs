using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe;

public static class KafeObjectFactoryExtensions
{
    extension(KafeObjectFactory f)
    {
        public KafeObject Wrap<T>(T value)
            where T : notnull
        {
            return f.Wrap(typeof(T), value);
        }

        public KafeObject Wrap(object value)
        {
            return f.Wrap(value.GetType(), value);
        }

        public ImmutableArray<KafeObject> WrapMany(params IEnumerable<object> values)
        {
            return [.. values.Select(f.Wrap)];
        }
    }
}

