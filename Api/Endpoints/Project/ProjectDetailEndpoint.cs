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
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project/{id}")]
public class ProjectDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectDetailDto>
{
    private readonly ProjectService projectService;
    private readonly EntityService entityService;
    private readonly UserProvider userProvider;
    private readonly IAuthorizationService authorizationService;

    public ProjectDetailEndpoint(
        ProjectService projectService,
        EntityService entityService,
        UserProvider userProvider,
        IAuthorizationService authorizationService)
    {
        this.projectService = projectService;
        this.entityService = entityService;
        this.userProvider = userProvider;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    [ProducesResponseType(typeof(ProjectDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var project = await projectService.Load(id, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        var userPerms = await entityService.GetPermission(project.Id, userProvider.Account?.Id, cancellationToken);
        return Ok(TransferMaps.ToProjectDetailDto(project, userPerms));
    }
}
