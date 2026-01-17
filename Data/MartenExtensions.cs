using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Kafe.Core.Diagnostics;
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
    extension(IEventOperations ops)
    {
        public StreamAction KafeStartStream<TAggregate>(
            Hrib id,
            IEnumerable<object> events
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
            IEnumerable<object> events
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
        public Task<T?> KafeAggregateStream<T>(
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
            return await store.KafeAggregateStream(
                    id: id,
                    version: version,
                    timestamp: timestamp,
                    state: state,
                    fromVersion: fromVersion,
                    token: token
                )
                ?? throw new InvalidOperationException($"{typeof(T).Name} '{id}' could not be live-aggregated.");
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

        public async Task<Err<T>> LoadAsync<T>(
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
        public async Task<Err<ImmutableArray<T>>> LoadManyAsync<T>(
            IReadOnlyList<Hrib> ids,
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
            var errors = ImmutableArray.CreateBuilder<Diagnostic>();
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
        public async Task<T> RequireLatest<T>(
            Hrib entityId,
            CancellationToken ct = default
        )
            where T : class, IEntity
        {
            return await db.FetchLatest<T>(entityId.ToString(), ct)
                ?? throw new InvalidOperationException(
                    $"The latest version of {typeof(T).Name} '{entityId}' could not be fetched."
                );
        }
    }
}
