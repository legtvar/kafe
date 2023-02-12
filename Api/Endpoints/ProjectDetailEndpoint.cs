using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        var authors = await Task.WhenAll(data.Authors
            .Select(a => db.Events.AggregateStreamAsync<Author>(a.Id, token: cancellationToken)));

        if (authors is null || authors.Any(a => a is null))
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

        var artifacts = await Task.WhenAll(data.ArtifactIds
            .Select(a => db.Events.AggregateStreamAsync<Artifact>(a, token: cancellationToken)));

        if (artifacts is null || artifacts.Any(a => a is null))
        {
            return NotFound();
        }

        var artifactsBuilder = ImmutableArray.CreateBuilder<ProjectArtifactDto>();
        foreach (var artifact in artifacts)
        {
            var shards = await Task.WhenAll(artifact!.ShardIds
                .Select(s => db.Events.AggregateStreamAsync<Shard>(s, token: cancellationToken)));
            if (shards is null || shards.Any(s => s is null))
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

        return Ok();
    }
}
