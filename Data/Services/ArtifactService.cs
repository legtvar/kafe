using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ArtifactService
{
    private readonly IKafeDocumentSession db;
    private readonly DiagnosticFactory diagnosticFactory;

    public ArtifactService(
        IKafeDocumentSession db,
        DiagnosticFactory diagnosticFactory
    )
    {
        this.db = db;
        this.diagnosticFactory = diagnosticFactory;
    }

    public async Task<Err<ArtifactInfo>> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ArtifactInfo>(id.ToString(), token);
    }

    public async Task<ImmutableArray<ArtifactInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<ArtifactInfo>(ids.ToImmutableArray(), token)).Unwrap();
    }

    public async Task<Err<ArtifactInfo>> Create(ArtifactInfo @new, CancellationToken token = default)
    {
        if (!Hrib.TryParse(@new.Id, out var id, out _))
        {
            return diagnosticFactory.FromPayload(new BadHribDiagnostic(@new.Id));
        }

        if (id == Hrib.Empty)
        {
            id = Hrib.Create();
        }

        var created = new ArtifactCreated(
            ArtifactId: id.ToString(),
            CreationMethod: @new.CreationMethod is not CreationMethod.Unknown
                ? @new.CreationMethod
                : CreationMethod.Api,
            Name: @new.Name,
            AddedOn: @new.AddedOn != default ? @new.AddedOn.ToUniversalTime() : DateTimeOffset.UtcNow
        );
        db.Events.KafeStartStream<ArtifactInfo>(created.ArtifactId, created);

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<ArtifactInfo>(id, token: token);
    }
}
