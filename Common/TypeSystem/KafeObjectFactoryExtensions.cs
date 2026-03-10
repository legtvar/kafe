using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe;

public static class KafeObjectFactoryExtensions
{
    extension(KafeObjectFactory factory)
    {
        public KafeObject Wrap<T>(T value)
            where T : notnull
        {
            return factory.Wrap(typeof(T), value);
        }

        public KafeObject Wrap(object value)
        {
            return factory.Wrap(value.GetType(), value);
        }

        public ImmutableArray<KafeObject> WrapMany(params IEnumerable<object> values)
        {
            return [.. values.Select(factory.Wrap)];
        }

        public ImmutableDictionary<string, KafeObject> WrapProperties(
            params IEnumerable<(string key, object? value)> properties
        )
        {
            var builder = ImmutableDictionary.CreateBuilder<string, KafeObject>();
            foreach (var (key, value) in properties)
            {
                if (value is null)
                {
                    // NB: Yes, (artifact) properties cannot be null. If you need it to, use some kind of "Empty" value.
                    continue;
                }

                // NB: if a property is set multiple times, the last (non-null) value wins
                builder[key] = factory.Wrap(value);
            }

            return builder.ToImmutable();
        }
    }
}
