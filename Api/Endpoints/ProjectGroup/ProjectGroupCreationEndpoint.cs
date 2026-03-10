using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group")]
[Authorize]
public class ProjectGroupCreationEndpoint(
    ProjectGroupService projectGroupService,
    IAuthorizationService authorizationService
)
    : EndpointBaseAsync
        .WithRequest<ProjectGroupCreationDto>
        .WithActionResult<Hrib>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectGroupCreationDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.OrganizationId, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var group = await projectGroupService.Create(
            ProjectGroupInfo.Create(dto.OrganizationId, dto.Name) with
            {
                Description = dto.Description,
                Deadline = dto.Deadline,
                IsOpen = dto.IsOpen,
                ValidationSettings = dto.OrganizationId == "mate-fimuni"
                    ? ProjectValidationSettings.MateValidationSettings
                    : null // TODO: temporary workaround until artifact blueprints are implemented
            },
            ct: cancellationToken
        );
        if (group.HasError)
        {
            return this.KafeErrResult(group);
        }

        return Ok((Hrib)group.Value.Id);
    }
}
