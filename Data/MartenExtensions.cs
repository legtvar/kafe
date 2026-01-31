using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Diagnostics;
using Kafe.Data.Documents;
using Marten;
using Marten.Events;
using Marten.Exceptions;

namespace Kafe.Data;

/// <summary>
/// Extensions that help enforce KAFE's custom data types.
/// </summary>
public static class MartenExtensions
{
    extension(IEventOperations ops)
    {
        public StreamAction KafeStartStream<TAggregate>(
            Hrib id,
            params IEnumerable<object> events
        ) where TAggregate : class, IEntity
        {
            return ops.StartStream<TAggregate>(streamKey: id.ToString(), events: events);
        }

        public StreamAction KafeStartStream<TAggregate>(
            Hrib id,
            params object[] events
        ) where TAggregate : class, IEntity
        {
            return ops.KafeStartStream<TAggregate>(id, events.AsEnumerable());
        }

        public StreamAction KafeAppend(
            Hrib streamId,
            params IEnumerable<object> events
        )
        {
            return ops.Append(streamId.ToString(), events);
        }

        public StreamAction KafeAppend(
            Hrib streamId,
            params object[] events
        )
        {
            return ops.KafeAppend(streamId, events.AsEnumerable());
        }
    }

    extension(IQueryEventStore store)
    {
        public async Task<Err<T>> KafeAggregateStream<T>(
            Hrib id,
            long version = 0L,
            DateTimeOffset? timestamp = null,
            T? state = null,
            long fromVersion = 0L,
            CancellationToken token = default
        )
            where T : class, IEntity
        {
            var aggregate = await store.AggregateStreamAsync(
                streamKey: id.ToString(),
                version: version,
                timestamp: timestamp,
                state: state,
                fromVersion: fromVersion,
                token: token
            );
            if (aggregate is null)
            {
                return Err.Fail(new NotFoundDiagnostic(typeof(T), id));
            }

            return aggregate;
        }

        public async Task<T> KafeAggregateRequiredStream<T>(
            Hrib id,
            long version = 0L,
            DateTimeOffset? timestamp = null,
            T? state = null,
            long fromVersion = 0L,
            CancellationToken token = default
        )
            where T : class, IEntity
        {
            return (await store.KafeAggregateStream(
                    id: id,
                    version: version,
                    timestamp: timestamp,
                    state: state,
                    fromVersion: fromVersion,
                    token: token
                )).Unwrap();
        }
    }

    extension(IQuerySession db)
    {
        public async Task<bool> TryWaitForEntityPermissions(
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

        public async Task<Err<T>> KafeLoadAsync<T>(
            Hrib id,
            CancellationToken token = default
        )
            where T : IEntity
        {
            var entity = await db.LoadAsync<T>(id.ToString(), token: token);
            if (entity is not null)
            {
                return entity;
            }

            return Err.Fail<T>(
                new NotFoundDiagnostic(
                    EntityType: typeof(T),
                    Id: id
                )
            );
        }

        /// <summary>
        /// Asynchronously loads entities of type <typeparamref name="T"/> specified by <paramref name="ids"/>.
        /// </summary>
        /// <remarks>
        /// Returns entities in the same order as in <paramref name="ids"/> and respects duplicates.
        /// Returns an <see cref="Kafe.Diagnostic"/>, if any of the ids cannot be found.
        /// Even in case of error, returns the entities that were found.
        /// </remarks>
        public async Task<Err<ImmutableArray<T>>> KafeLoadManyAsync<T>(
            IReadOnlyList<Hrib> ids,
            CancellationToken token = default
        ) where T : IEntity
        {
            var stringIds = ids.Select(i => (string)i).ToImmutableArray();

            var entities = (await db.LoadManyAsync<T>(
                    token: token,
                    ids: stringIds
                ))
                .ToImmutableArray()
                .SortEntitiesBy(ids);
            if (entities.Length != ids.Count)
            {
                var missingIds = ids.Except(entities.Select(e => e.Id)).ToImmutableArray();
                var notFoundErrors = Kafe.Diagnostic.Aggregate(
                    missingIds.Select(id => new NotFoundDiagnostic(
                            EntityType: typeof(T),
                            Id: id
                        )
                    )
                );
                return (entities, notFoundErrors);
            }

            return entities;
        }
    }

    extension(IEventStoreOperations db)
    {
        public async Task<Err<T>> KafeFetchLatest<T>(
            Hrib entityId,
            CancellationToken ct = default
        )
            where T : class, IEntity
        {
            var entity = await db.FetchLatest<T>(entityId.ToString(), ct);
            if (entity is null)
            {
                return Err.Fail(new NotFoundDiagnostic(typeof(T), entityId));
            }

            return entity;
        }

        public async Task<T> RequireLatest<T>(
            Hrib entityId,
            CancellationToken ct = default
        )
            where T : class, IEntity
        {
            return (await db.KafeFetchLatest<T>(entityId, ct)).Unwrap();
        }

        public async Task<Err<IEventStream<T>>> KafeFetchForWriting<T>(
            Hrib entityId,
            CancellationToken ct = default
        )
            where T : class, IEntity
        {
            var eventStream = await db.FetchForWriting<T>(entityId.ToString(), ct);
            if (eventStream.Aggregate is null)
            {
                return Err.Fail(new NotFoundDiagnostic(typeof(T), entityId));
            }

            return new Err<IEventStream<T>>(eventStream);
        }

        public async Task<Err<IEventStream<T>>> KafeFetchForExclusiveWriting<T>(
            Hrib entityId,
            CancellationToken ct = default
        )
            where T : class, IEntity
        {
            try
            {
                var eventStream = await db.FetchForExclusiveWriting<T>(entityId.ToString(), ct);
                if (eventStream.Aggregate is null)
                {
                    return Err.Fail(new NotFoundDiagnostic(typeof(T), entityId));
                }

                return new Err<IEventStream<T>>(eventStream);
            }
            catch (StreamLockedException)
            {
                return Err.Fail(new EntityBusyDiagnostic(typeof(T), entityId));
            }
        }
    }
}
