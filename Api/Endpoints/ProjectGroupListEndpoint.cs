using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("project-groups")]
[Authorize]
public class ProjectGroupListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<ProjectGroupListDto>>
{
    private readonly IQuerySession db;

    public ProjectGroupListEndpoint(IQuerySession db)
    {
        this.db = db;
    }
    
    [HttpGet]
    public override async Task<ActionResult<List<ProjectGroupListDto>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projectGroups = await db.Query<ProjectGroup>().ToListAsync(cancellationToken);
        return Ok(projectGroups.Select(TransferMaps.ToProjectGroupListDto).ToList());
    }
}
