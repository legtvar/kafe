using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group")]
[Authorize(EndpointPolicy.Append)]
public class ProjectGroupCreationEndpoint : EndpointBaseAsync
    .WithRequest<ProjectGroupCreationDto>
    .WithActionResult<Hrib>
{
    private readonly ProjectGroupService projectGroupService;
    private readonly IAuthorizationService authorizationService;

    public ProjectGroupCreationEndpoint(
        ProjectGroupService projectGroupService,
        IAuthorizationService authorizationService)
    {
        this.projectGroupService = projectGroupService;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    [Tags(EndpointArea.ProjectGroup)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectGroupCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.OrganizationId, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var group = await projectGroupService.Create(ProjectGroupInfo.Create(dto.OrganizationId, dto.Name) with
        {
            Description = dto.Description,
            Deadline = dto.Deadline,
            IsOpen = dto.IsOpen
        }, cancellationToken);
        if (group.HasErrors)
        {
            return this.KafeErrResult(group);
        }

        return Ok((Hrib)group.Value.Id);
    }
}
