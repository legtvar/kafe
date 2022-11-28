using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("project-groups")]
public class ProjectGroupListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<ProjectGroup>>
{
    private readonly IQuerySession db;

    public ProjectGroupListEndpoint(IQuerySession db)
    {
        this.db = db;
    }
    
    [HttpGet]
    public override async Task<ActionResult<List<ProjectGroup>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await db.Query<ProjectGroup>().ToListAsync());
    }
}
