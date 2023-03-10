using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project/{id}")]
[Authorize]
public class ProjectDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectDetailDto>
{
    private readonly IProjectService projects;

    public ProjectDetailEndpoint(IProjectService projects)
    {
        this.projects = projects;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    [ProducesResponseType(typeof(ProjectDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.Load(id, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }
}
