using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Marten.Events;

namespace Kafe.Data;

/// <summary>
/// Extensions that help enforce KAFE's custom data types.
/// </summary>
public static class MartenExtensions
{
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
