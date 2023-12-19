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
    .WithActionResult
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
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    public override async Task<ActionResult> HandleAsync(
        ProjectGroupEditDto request,
        CancellationToken cancellationToken = default)
    {
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
            Deadline = request.Deadline ?? @old.Deadline
        };
        var result = await projectGroupService.Edit(@new, cancellationToken);
        if (result.HasErrors)
        {
            return ValidationProblem(title: result.Errors.FirstOrDefault());
        }

        return Ok();
    }
}
