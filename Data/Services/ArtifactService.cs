using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
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

    public async Task<Err<ArtifactInfo>> Upsert(
        ArtifactInfo artifact,
        CancellationToken ct = default
    )
    {
        var idErr = Hrib.TryParseValid(artifact.Id, shouldReplaceEmpty: true);
        if (idErr.HasError)
        {
            return idErr.Diagnostic;
        }

        var id = idErr.Value;
        var existingErr = await db.Events.KafeFetchForWriting<ArtifactInfo>(id, ct);
        if (existingErr is { HasError: true, Diagnostic.Payload: not NotFoundDiagnostic })
        {
            return existingErr.Diagnostic;
        }

        if (existingErr is { HasError: true, Diagnostic.Payload: NotFoundDiagnostic })
        {
            var created = new ArtifactCreated(
                ArtifactId: id.ToString(),
                CreationMethod: artifact.CreationMethod is not CreationMethod.Unknown
                    ? artifact.CreationMethod
                    : CreationMethod.Api,
                Name: artifact.Name,
                AddedOn: artifact.AddedOn != default ? artifact.AddedOn.ToUniversalTime() : DateTimeOffset.UtcNow
            );
            db.Events.KafeStartStream<ArtifactInfo>(created.ArtifactId, created);
            await db.SaveChangesAsync(ct);
            existingErr = await db.Events.KafeFetchForWriting<ArtifactInfo>(id, ct);
        }

        if (existingErr.HasError)
        {
            return existingErr.Diagnostic;
        }

        var existing = existingErr.Value;
        // TODO: Add some mechanism to change
        var changedProperties = artifact.Properties
            .Where(p =>
                !existing.Aggregate!.Properties.ContainsKey(p.Key)
                || existing.Aggregate.Properties[p.Key] != artifact.Properties[p.Key]
            )
            .Select(p => new KeyValuePair<string, ArtifactPropertySetter>(
                    p.Key,
                    new ArtifactPropertySetter(p.Value, ExistingValueHandling.OverwriteExisting)
                )
            )
            .Concat(
                existing.Aggregate!.Properties
                    .Where(p => !artifact.Properties.ContainsKey(p.Key))
                    .Select(p => new KeyValuePair<string, ArtifactPropertySetter>(
                            p.Key,
                            new ArtifactPropertySetter(null, ExistingValueHandling.OverwriteExisting)
                        )
                    )
            )
            .ToImmutableDictionary();
        if (changedProperties.Count > 0)
        {
            existing.AppendOne(new ArtifactPropertiesSet(id.ToString(), changedProperties));
        }

        await db.SaveChangesAsync(ct);
        return await db.Events.KafeAggregateRequiredStream<ArtifactInfo>(id, token: ct);
    }

    public async Task<Err<ImmutableArray<ProjectInfo>>> GetContainingProjects(
        Hrib id,
        CancellationToken ct = default
    )
    {
        var artifactErr = await Load(id, ct);
        if (artifactErr.HasError)
        {
            return artifactErr.Diagnostic;
        }

        var containingProjects = await db.Query<ProjectInfo>()
            .Where(g => g.MatchesSql(
                    $"""
                     EXISTS(
                        SELECT a.* FROM {db.DocumentStore.Options.Schema.For<ProjectInfo>(true)} AS a
                        WHERE a.data ->> '{nameof(ProjectInfo.ArtifactId)}' = ?
                     """,
                    id.ToString()
                )
            )
            .ToListAsync(ct);
        return containingProjects.ToImmutableArray();
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
