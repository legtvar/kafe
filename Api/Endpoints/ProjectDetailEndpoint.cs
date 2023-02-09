using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        var group = await db.Events.AggregateStreamAsync<ProjectGroup>(data.ProjectGroupId, token: cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToProjectDetailDto(data));
    }
}
