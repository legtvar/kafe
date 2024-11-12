using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data.Aggregates;

namespace Kafe.Data;

public static class EntitySortExtensions
{
    /// <summary>
    /// Sorts a list of entities according to an order defined by a list of Ids.
    /// </summary>
    /// <remarks>
    /// Entities without a defined <paramref name="order"/> are ignored.
    /// Respects duplicate Ids in <paramref name="order"/>.
    /// If <paramref name="entities"/> contains duplicates, the *last* entity for a given Id is used.
    /// If <paramref name="order"/> contains an Id without a counterpart in <paramref name="entities"/>,
    /// nothing is appended to the resulting array.
    /// </remarks>
    public static ImmutableArray<T> SortEntitiesBy<T>(
        this IReadOnlyList<T> entities,
        IReadOnlyList<Hrib> order
    ) where T : IEntity
    {
        var mapBuilder = ImmutableDictionary.CreateBuilder<Hrib, T>();
        foreach (var entity in entities)
        {
            mapBuilder[entity.Id] = entity;
        }
        var map = mapBuilder.ToImmutable();

        var resultBuilder = ImmutableArray.CreateBuilder<T>(order.Count);
        foreach (var id in order)
        {
            if (map.TryGetValue(id, out var entity))
            {
                resultBuilder.Add(entity);
            }
        }
        return resultBuilder.ToImmutable();
    }
}
