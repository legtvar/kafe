using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ArtifactService
{
    private readonly IDocumentSession db;

    public ArtifactService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<ArtifactInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ArtifactInfo>(id.ToString(), token);
    }

    public async Task<ImmutableArray<ArtifactInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<ArtifactInfo>(token, ids.Select(i => (string)i)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }

    public async Task<ArtifactDetail?> LoadDetail(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ArtifactDetail>(id.ToString(), token);
    }

    public async Task<ImmutableArray<ArtifactDetail>> LoadDetailMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<ArtifactDetail>(token, ids.Select(i => (string)i)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }

    public async Task<Err<ArtifactInfo>> Create(ArtifactInfo @new, CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var id = parseResult.Value;
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
