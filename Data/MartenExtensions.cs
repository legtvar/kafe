using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Marten;
using Marten.Events;

namespace Kafe.Data;

/// <summary>
/// Extensions that help enforce KAFE's custom data types.
/// </summary>
public static class MartenExtensions
{
    public static async Task<Err<T>> KafeLoadAsync<T>(
        this IQuerySession db,
        Hrib id,
        CancellationToken token = default
    )
        where T : notnull, IEntity
    {
        var entity = await db.LoadAsync<T>(id.ToString(), token: token);
        if (entity is not null)
        {
            return entity;
        }

        return Error.NotFound(id, DataConst.GetLocalizedName(typeof(T)).Invariant);
    }

    /// <summary>
    /// Asynchronously loads entities of type <typeparamref name="T"/> specified by <paramref name="ids"/>.
    /// </summary>
    /// <remarks>
    /// Returns entities in the same order as in <paramref name="ids"/> and respects duplicates.
    /// Returns an <see cref="Error"/>, if any of the ids cannot be found.
    /// Even in case of error, returns the entities that were found.
    /// </remarks>
    public static async Task<Err<ImmutableArray<T>>> KafeLoadManyAsync<T>(
        this IQuerySession db,
        ImmutableArray<Hrib> ids,
        CancellationToken token = default
    ) where T : notnull, IEntity
    {
        var stringIds = ids.Select(i => (string)i).ToImmutableArray();
        var entities = (await db.LoadManyAsync<T>(
                token: token,
                ids: stringIds
            ))
            .ToImmutableArray()
            .SortEntitiesBy(ids);
        if (entities.Length != ids.Length)
        {
            var missingIds = stringIds.Except(entities.Select(e => e.Id)).ToImmutableArray();
            var notFoundError = Error.NotFound(
                "Some of the sought entities could not be found: "
                + $"{string.Join(", ", missingIds)}."
            );
            return (entities, notFoundError);
        }

        return entities;
    }

    public static StreamAction KafeStartStream<TAggregate>(
        this IEventOperations ops,
        Hrib id,
        IEnumerable<object> events
    ) where TAggregate : class, IEntity
    {
        return ops.StartStream<TAggregate>(streamKey: id.ToString(), events: events);
    }

    public static StreamAction KafeStartStream<TAggregate>(
        this IEventOperations ops,
        Hrib id,
        params object[] events
    ) where TAggregate : class, IEntity
    {
        return KafeStartStream<TAggregate>(ops, id, events.AsEnumerable());
    }

    public static StreamAction KafeAppend(
        this IEventOperations ops,
        Hrib streamId,
        IEnumerable<object> events
    )
    {
        return ops.Append(streamId.ToString(), events);
    }

    public static StreamAction KafeAppend(
        this IEventOperations ops,
        Hrib streamId,
        params object[] events
    )
    {
        return KafeAppend(ops, streamId, events.AsEnumerable());
    }

    public static Task<T?> KafeAggregateStream<T>(
        this IQueryEventStore store,
        Hrib id,
        long version = 0L,
        DateTimeOffset? timestamp = null,
        T? state = null,
        long fromVersion = 0L,
        CancellationToken token = default
    )
        where T : class, IEntity
    {
        return store.AggregateStreamAsync(
            streamKey: id.ToString(),
            version: version,
            timestamp: timestamp,
            state: state,
            fromVersion: fromVersion,
            token: token
        );
    }

    public static async Task<T> KafeAggregateRequiredStream<T>(
        this IQueryEventStore store,
        Hrib id,
        long version = 0L,
        DateTimeOffset? timestamp = null,
        T? state = null,
        long fromVersion = 0L,
        CancellationToken token = default
    )
        where T : class, IEntity
    {
        return await KafeAggregateStream(
                store: store,
                id: id,
                version: version,
                timestamp: timestamp,
                state: state,
                fromVersion: fromVersion,
                token: token
            )
            ?? throw new InvalidOperationException($"{typeof(T).Name} '{id}' could not be live-aggregated.");
    }

    public static async Task<bool> TryWaitForEntityPermissions(
        this IQuerySession db,
        Hrib entityId,
        CancellationToken ct = default
    )
    {
        try
        {
            await db.QueryForNonStaleData<EntityPermissionInfo>(TimeSpan.FromSeconds(5))
                .Where(i => i.Id == entityId.ToString())
                .SingleOrDefaultAsync(ct);
            return true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
