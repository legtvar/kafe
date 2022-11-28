using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("projects")]
public class ProjectListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<Project>>
{
    private readonly IQuerySession db;

    public ProjectListEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    public override async Task<ActionResult<List<Project>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await db.Query<Project>().ToListAsync());
    }
}
