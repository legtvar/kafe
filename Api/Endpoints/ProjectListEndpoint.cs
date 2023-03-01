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
using System.Collections.Immutable;
using Kafe.Api.Services;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("projects")]
[Authorize]
public class ProjectListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<ProjectListDto>>
{
    private readonly IProjectService projects;

    public ProjectListEndpoint(IProjectService projects)
    {
        this.projects = projects;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Project })]
    public override async Task<ActionResult<ImmutableArray<ProjectListDto>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await projects.List(cancellationToken));
    }
}
