using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;

namespace Kafe.Data;

public partial class KafeDocumentSession : IKafeQuerySession
{
    private readonly KafeTypeRegistry typeRegistry;
    private readonly DiagnosticFactory diagnosticFactory;

    public async Task<Err<T>> LoadAsync<T>(
        Hrib id,
        CancellationToken token = default)
        where T : notnull, IEntity
    {
        var kafeType = typeRegistry.RequireType<T>();
        var entity = await Inner.LoadAsync<T>(id.ToString(), token: token);
        if (entity is not null)
        {
            return entity;
        }

        return diagnosticFactory.FromPayload(new NotFoundDiagnostic(
            EntityType: kafeType,
            Id: id
        ));
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
        ImmutableArray<Hrib> ids,
        CancellationToken token = default
    ) where T : notnull, IEntity
    {
        var kafeType = typeRegistry.RequireType<T>();

        var stringIds = ids.Select(i => (string)i).ToImmutableArray();

        var entities = (await LoadManyAsync<T>(
                token: token,
                ids: stringIds
            ))
            .ToImmutableArray()
            .SortEntitiesBy(ids);
        var errors = ImmutableArray.CreateBuilder<Diagnostic>();
        if (entities.Length != ids.Length)
        {
            var missingIds = stringIds.Except(entities.Select(e => e.Id)).ToImmutableArray();
            var notFoundErrors = missingIds.Select(id => diagnosticFactory.FromPayload(new NotFoundDiagnostic(
                EntityType: kafeType,
                Id: id
            ))).ToImmutableArray();
            var finalDiagnostic = notFoundErrors.Length > 1
                ? diagnosticFactory.FromPayload(new AggregateDiagnostic(notFoundErrors))
                : notFoundErrors.Single();
            return (entities, finalDiagnostic);
        }

        return entities;
    }
}
