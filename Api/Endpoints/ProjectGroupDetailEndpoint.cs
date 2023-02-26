using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1.0")]
[Route("project-group/{id}")]
[Authorize]
public class ProjectGroupDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectGroupDetailDto>
{
    private readonly IQuerySession db;

    public ProjectGroupDetailEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    public override async Task<ActionResult<ProjectGroupDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var data = await db.Events.AggregateStreamAsync<ProjectGroupInfo>(id, token: cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToProjectGroupDetailDto(data));
    }
}
