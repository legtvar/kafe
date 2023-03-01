using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Api.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
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
    [SwaggerOperation(Tags = new[] { SwaggerTags.ProjectGroup })]
    public override async Task<ActionResult<List<ProjectGroupListDto>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projectGroups = await db.Query<ProjectGroupInfo>().ToListAsync(cancellationToken);
        return Ok(projectGroups.Select(TransferMaps.ToProjectGroupListDto).ToList());
    }
}
