using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Common;
using Kafe.Data.Aggregates;
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
        CancellationToken token = default)
        where T : notnull, IEntity
    {
        var entity = await db.LoadAsync<T>(id.ToString(), token: token);
        if (entity is not null)
        {
            return entity;
        }

        return Error.NotFound(id, DataConst.GetLocalizedName(typeof(T)).Invariant);
    }

    public static async Task<Err<ImmutableArray<T>>> KafeLoadManyAsync<T>(
        this IQuerySession db,
        ImmutableArray<Hrib> ids,
        CancellationToken token = default
    ) where T : notnull, IEntity
    {
        var entities = (await db.LoadManyAsync<T>(
            token: token,
            ids: ids.Select(i => i.ToString())
        )).ToImmutableArray();
        if (entities.Length != ids.Length || entities.Any(e => e is null))
        {
            return Error.NotFound("Some of the entities could not be found.");
        }

        return entities;
    }

    public static StreamAction KafeStartStream<TAggregate>(
        this IEventOperations ops,
        Hrib id,
        IEnumerable<object> events) where TAggregate : class, IEntity
    {
        return ops.StartStream<TAggregate>(streamKey: id.ToString(), events: events);
    }

    public static StreamAction KafeStartStream<TAggregate>(
        this IEventOperations ops,
        Hrib id,
        params object[] events) where TAggregate : class, IEntity
    {
        return KafeStartStream<TAggregate>(ops, id, events.AsEnumerable());
    }

    public static StreamAction KafeAppend(
        this IEventOperations ops,
        Hrib streamId,
        IEnumerable<object> events)
    {
        return ops.Append(streamId.ToString(), events);
    }

    public static StreamAction KafeAppend(
        this IEventOperations ops,
        Hrib streamId,
        params object[] events)
    {
        return KafeAppend(ops, streamId, events.AsEnumerable());
    }

    public static Task<T?> KafeAggregateStream<T>(
        this IEventStore store,
        Hrib id,
        long version = 0L,
        DateTimeOffset? timestamp = null,
        T? state = null,
        long fromVersion = 0L,
        CancellationToken token = default)
        where T : class, IEntity
    {
        return store.AggregateStreamAsync(
            streamKey: id.ToString(),
            version: version,
            timestamp: timestamp,
            state: state,
            fromVersion: fromVersion,
            token: token);
    }

    public static async Task<T> KafeAggregateRequiredStream<T>(
        this IEventStore store,
        Hrib id,
        long version = 0L,
        DateTimeOffset? timestamp = null,
        T? state = null,
        long fromVersion = 0L,
        CancellationToken token = default)
        where T : class, IEntity
    {
        return await KafeAggregateStream(
            store: store,
            id: id,
            version: version,
            timestamp: timestamp,
            state: state,
            fromVersion: fromVersion,
            token: token)
            ?? throw new InvalidOperationException($"{typeof(T).Name} '{id}' could not be live-aggregated.");
    }
}
