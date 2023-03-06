using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group")]
[Authorize(Policy = EndpointPolicy.AdministratorOnly)]
public class ProjectGroupCreationEndpoint : EndpointBaseAsync
    .WithRequest<ProjectGroupCreationDto>
    .WithActionResult<Hrib>
{
    private readonly IProjectGroupService projectGroups;

    public ProjectGroupCreationEndpoint(IProjectGroupService projectGroups)
    {
        this.projectGroups = projectGroups;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectGroupCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var group = await projectGroups.Create(dto, cancellationToken);
        return Ok(group);
    }
}
