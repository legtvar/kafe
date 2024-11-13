using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project-group")]
[Authorize]
public class ProjectGroupEditEndpoint : EndpointBaseAsync
    .WithRequest<ProjectGroupEditDto>
    .WithActionResult<Hrib>
{
    private readonly ProjectGroupService projectGroupService;
    private readonly IAuthorizationService authorizationService;

    public ProjectGroupEditEndpoint(
        ProjectGroupService projectGroupService,
        IAuthorizationService authorizationService)
    {
        this.projectGroupService = projectGroupService;
        this.authorizationService = authorizationService;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectGroupEditDto request,
        CancellationToken cancellationToken = default)
    {
        // TODO: Add project group permission to edit child project permissions. Right now it's disabled globally.
        // NB: Remove this segment once you fix this.
        var auth = await authorizationService.AuthorizeAsync(User, request.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var @old = await projectGroupService.Load(request.Id, token: cancellationToken);
        if (@old is null)
        {
            return NotFound();
        }

        var @new = @old with
        {
            Name = request.Name ?? @old.Name,
            Description = request.Description ?? @old.Description,
            IsOpen = request.IsOpen ?? @old.IsOpen,
            Deadline = request.Deadline ?? @old.Deadline,
            OrganizationId = request.OrganizationId?.ToString() ?? @old.OrganizationId
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
        if (result.HasErrors)
        {
            return result.ToActionResult();
        }

        return Ok((Hrib)result.Value.Id);
    }
}
