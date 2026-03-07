using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
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
public class ProjectGroupEditEndpoint(
    ProjectGroupService projectGroupService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ProjectGroupEditDto>
    .WithActionResult<Hrib>
{
    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectGroupEditDto request,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: Add project group permission to edit child project permissions. Right now it's disabled globally.
        // NB: Remove this segment once you fix this.
        var auth = await authorizationService.AuthorizeAsync(User, request.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var oldErr = await projectGroupService.Load(request.Id, token: cancellationToken);
        if (oldErr.HasError)
        {
            return this.KafeErrResult(oldErr);
        }

        var old = oldErr.Value;
        var @new = old with
        {
            Name = request.Name ?? @old.Name,
            Description = request.Description ?? @old.Description,
            IsOpen = request.IsOpen ?? @old.IsOpen,
            Deadline = request.Deadline ?? @old.Deadline,
            OrganizationId = request.OrganizationId?.ToString() ?? @old.OrganizationId,
            ValidationSettings = request.ValidationSettings ?? @old.ValidationSettings
        };

        if (@new.OrganizationId != old.OrganizationId)
        {
            var oldOrgAuth = await authorizationService.AuthorizeAsync(User, old.OrganizationId, EndpointPolicy.Write);
            if (!oldOrgAuth.Succeeded)
            {
                return Unauthorized($"You don't have write permissions to organization '{old.OrganizationId}'.");
            }

            var newOrgAuth = await authorizationService.AuthorizeAsync(User, @new.OrganizationId, EndpointPolicy.Write);
            if (!newOrgAuth.Succeeded)
            {
                return Unauthorized($"You don't have write permissions to organization '{@new.OrganizationId}'.");
            }
        }

        var result = await projectGroupService.Edit(@new, cancellationToken);
        if (result.HasError)
        {
            return this.KafeErrResult(result);
        }

        return Ok((Hrib)result.Value.Id);
    }
}
