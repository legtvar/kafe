using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group/{id}")]
[Authorize]
public class ProjectGroupDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectGroupDetailDto>
{
    private readonly IProjectGroupService projectGroups;

    public ProjectGroupDetailEndpoint(IProjectGroupService projectGroups)
    {
        this.projectGroups = projectGroups;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    [ProducesResponseType(typeof(ProjectGroupDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectGroupDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var dto = await projectGroups.Load(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
