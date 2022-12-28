using System.Linq;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("projects")]
[Authorize]
public class ProjectListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<ProjectListDto>>
{
    private readonly IQuerySession db;

    public ProjectListEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    public override async Task<ActionResult<List<ProjectListDto>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await db.Query<Project>().ToListAsync(cancellationToken);
        return Ok(projects.Select(TransferMaps.ToProjectListDto).ToList());
    }
}
