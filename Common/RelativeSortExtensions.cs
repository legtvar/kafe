using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kafe;

public static class RelativeSortExtensions
{
    public static ImmutableArray<T> RelativeSortBy<T, K>(
        this IReadOnlyList<T> values,
        IReadOnlyList<K> keys,
        Func<T, K> keySelector
    ) where K : notnull
    {
        var indicesBuilder = ImmutableDictionary.CreateBuilder<K, int>();
        for (int i = 0; i < keys.Count; ++i)
        {
            if (indicesBuilder.ContainsKey(keys[i]))
            {
                throw new ArgumentException("The keys array must not contain duplicates.", nameof(keys));
            }

            indicesBuilder.Add(keys[i], i);
        }
        var indices = indicesBuilder.ToImmutable();

        return values.ToImmutableArray().Sort((a, b) =>
        {
            var aKey = keySelector(a);
            var bKey = keySelector(b);
            var aHasIndex = indices.TryGetValue(aKey, out var aIndex);
            var bHasIndex = indices.TryGetValue(bKey, out var bIndex);
            if (aHasIndex && !bHasIndex)
            {
                return -aIndex;
            }

            if (!aHasIndex && bHasIndex)
            {
                return bIndex;
            }

            return aIndex - bIndex;
        });
    }
}
