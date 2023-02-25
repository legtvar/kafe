using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("project/{id}")]
[Authorize]
public class ProjectDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectDetailDto>
{
    private readonly IQuerySession db;

    public ProjectDetailEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    public override async Task<ActionResult<ProjectDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var data = await db.Events.AggregateStreamAsync<Project>(id, token: cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        var dto = TransferMaps.ToProjectDetailDto(data);

        var group = await db.Events.AggregateStreamAsync<ProjectGroup>(data.ProjectGroupId, token: cancellationToken);
        if (group is null)
        {
            return NotFound();
        }


        dto = dto with { ProjectGroupName = group.Name };

        var authors = new List<Author?>();
        foreach (var projectAuthor in data.Authors)
        {
            authors.Add(await db.Events.AggregateStreamAsync<Author>(projectAuthor.Id, token: cancellationToken));
        }

        if (authors.Any(a => a is null))
        {
            return NotFound();
        }

        dto = dto with
        {
            Cast = data.Authors.Where(a => a.Kind == ProjectAuthorKind.Cast)
                .Select(a => new ProjectAuthorDto(
                    Id: a.Id,
                    Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
                    Roles: a.Roles))
                .ToImmutableArray(),
            Crew = data.Authors.Where(a => a.Kind == ProjectAuthorKind.Crew)
                .Select(a => new ProjectAuthorDto(
                    Id: a.Id,
                    Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
                    Roles: a.Roles))
                .ToImmutableArray()
        };

        var artifacts = new List<Artifact?>();
        foreach (var artifactId in data.ArtifactIds)
        {
            artifacts.Add(await db.Events.AggregateStreamAsync<Artifact>(artifactId, token: cancellationToken));
        }

        if (artifacts.Any(a => a is null))
        {
            return NotFound();
        }

        var artifactsBuilder = ImmutableArray.CreateBuilder<ProjectArtifactDto>();
        foreach (var artifact in artifacts)
        {
            var shards = new List<Shard?>();
            foreach (var shardId in artifact!.ShardIds)
            {
                shards.Add(await db.Events.AggregateStreamAsync<Shard>(shardId, token: cancellationToken));
            }

            if (shards.Any(s => s is null))
            {
                return NotFound();
            }

            artifactsBuilder.Add(new ProjectArtifactDto(
                Id: artifact.Id,
                Name: artifact.Name,
                Shards: shards.Select(s => new ProjectArtifactShardDto(
                    Id: s!.Id,
                    Kind: s.Kind,
                    Variants: s.GetVariants()))
                .ToImmutableArray()));
        }

        dto = dto with
        {
            Artifacts = artifactsBuilder.ToImmutable()
        };

        return Ok(dto);
    }
}
