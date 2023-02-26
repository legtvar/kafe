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
        var data = await db.LoadAsync<ProjectInfo>(id, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }


        var group = await db.LoadAsync<ProjectGroupInfo>(data.ProjectGroupId, cancellationToken);
        var artifactDetails = await db.LoadManyAsync<ArtifactDetail>(cancellationToken, data.ArtifactIds);
        var authors = await db.LoadManyAsync<AuthorInfo>(cancellationToken, data.Authors.Select(a => a.Id));
        var dto = TransferMaps.ToProjectDetailDto(data) with
        {
            ProjectGroupName = group?.Name ?? Const.UnknownProjectGroup,
            Artifacts = artifactDetails.Select(TransferMaps.ToProjectArtifactDto).ToImmutableArray(),
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

        return Ok(dto);
    }
}
