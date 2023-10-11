using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;
using System.Linq;
using System.Collections.Immutable;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group/{id}")]
public class ProjectGroupDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectGroupDetailDto>
{
    private readonly ProjectGroupService projectGroupService;
    private readonly ProjectService projectService;
    private readonly IAuthorizationService authorizationService;
    private readonly UserProvider userProvider;

    public ProjectGroupDetailEndpoint(
        ProjectGroupService projectGroupService,
        ProjectService projectService,
        IAuthorizationService authorizationService,
        UserProvider userProvider)
    {
        this.projectGroupService = projectGroupService;
        this.projectService = projectService;
        this.authorizationService = authorizationService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    [ProducesResponseType(typeof(ProjectGroupDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectGroupDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.ReadInspect);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
        var projectGroup = await projectGroupService.Load(id, cancellationToken);
        if (projectGroup is null)
        {
            return NotFound();
        }
        
        var dto = TransferMaps.ToProjectGroupDetailDto(projectGroup);
        var projects = await projectService.List(new(ProjectGroupId: projectGroup.Id), token: cancellationToken);
        var preferredCulture = userProvider.Account?.PreferredCulture ?? Const.InvariantCultureCode;
        dto = dto with
        {
            Projects = projects
                .OrderBy(p => ((LocalizedString)p.Name)[preferredCulture])
                .Select(TransferMaps.ToProjectListDto)
                .ToImmutableArray()
        };
        
        return Ok(dto);
    }
}
