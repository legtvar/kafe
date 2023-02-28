using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kafe.Api.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
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
    [SwaggerOperation(Tags = new[] { SwaggerTags.Project })]
    public override async Task<ActionResult<List<ProjectListDto>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await db.Query<ProjectInfo>().ToListAsync(cancellationToken);
        return Ok(projects.Select(TransferMaps.ToProjectListDto).ToList());
    }
}
