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

    public async Task<ArtifactDetail?> Load(Hrib id, CancellationToken token = default)
    {
        var artifact = await db.LoadAsync<ArtifactDetail>(id, token);
        if (artifact is null)
        {
            return null;
        }

        return artifact;
    }

    public async Task<ImmutableArray<ArtifactDetail>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<ArtifactDetail>(token, ids.Select(i => (string)i)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }

    public async Task<Hrib> Create(
        LocalizedString name,
        DateTimeOffset? addedOn,
        Hrib? containingProject,
        string? blueprintSlot,
        CancellationToken token = default)
    {
        var created = new ArtifactCreated(
            ArtifactId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            Name: name,
            AddedOn: addedOn?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            Visibility: Visibility.Public // artifact can be reference from any project or playlist
        );
        db.Events.StartStream<ArtifactInfo>(created.ArtifactId, created);

        if (containingProject is not null)
        {
            var artifactAdded = new ProjectArtifactAdded(
                ProjectId: containingProject,
                ArtifactId: created.ArtifactId,
                BlueprintSlot: blueprintSlot);
            var projectStream = await db.Events.FetchForWriting<ProjectInfo>(containingProject, token);
            projectStream.AppendOne(artifactAdded);
        }

        await db.SaveChangesAsync(token);
        return created.ArtifactId;
    }
}
