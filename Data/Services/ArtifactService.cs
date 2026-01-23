using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ArtifactService(
    IDocumentSession db
)
{
    public async Task<Err<ArtifactInfo>> Load(Hrib id, CancellationToken token = default)
    {
        return await db.KafeLoadAsync<ArtifactInfo>(id, token);
    }

    public async Task<Err<ImmutableArray<ArtifactInfo>>> LoadMany(
        IReadOnlyList<Hrib> ids,
        CancellationToken token = default)
    {
        return await db.KafeLoadManyAsync<ArtifactInfo>(ids, token);
    }

    public async Task<Err<ArtifactInfo>> Create(ArtifactInfo @new, CancellationToken token = default)
    {
        if (!Hrib.TryParse(@new.Id, out var id, out _))
        {
            return Err.Fail<ArtifactInfo>(new BadHribDiagnostic(@new.Id));
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
