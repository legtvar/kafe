using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten.Linq.MatchesSql;

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
        CancellationToken token = default
    )
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

    public record ContainingProjectGroupInfo(
        Hrib Id,
        LocalizedString Name
    );

    public async Task<Err<ImmutableArray<ContainingProjectGroupInfo>>> GetContainingProjectGroups(
        Hrib id,
        CancellationToken ct = default
    )
    {
        var artifactErr = await Load(id, ct);
        if (artifactErr.HasError)
        {
            return artifactErr.Diagnostic;
        }

        var containingProjectGroups = await db.Query<ProjectGroupInfo>()
            .Where(g => g.MatchesSql(
                    $"""
                     EXISTS(
                        SELECT a.* FROM {db.DocumentStore.Options.Schema.For<ProjectInfo>(true)} AS a
                        WHERE a.data ->> '{nameof(ProjectInfo.ProjectGroupId)}' = d.id
                            AND a.data ->> '{nameof(ProjectInfo.ArtifactId)}' = ?
                     """,
                    id.ToString()
                )
            )
            .ToListAsync(ct);

        return containingProjectGroups.DistinctBy(g => g.Id).Select(g => new ContainingProjectGroupInfo(
                Id: Hrib.Parse(g.Id),
                Name: g.Name
            )
        ).ToImmutableArray();
    }
}
