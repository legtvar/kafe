using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group/lock")]
public class ProjectGroupLockEndpoint(
    ProjectService projectService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ProjectGroupLockEndpoint.LockRequestData>
    .WithActionResult
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    public override async Task<ActionResult> HandleAsync(
        [FromBody] LockRequestData requestData,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, requestData.Id, EndpointPolicy.Administer);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var projects = await projectService.List(
            new ProjectService.ProjectFilter(ProjectGroupId: requestData.Id),
            token: ct
        );
        var err = new Err<bool>();
        foreach (var project in projects)
        {
            if (!project.IsLocked)
            {
                var result = await projectService.Lock(project.Id, ct);
                if (result.HasError)
                {
                    err = err.Combine(result.Diagnostic);
                }
            }
        }

        if (err.HasError)
        {
            return this.KafeErrorResult(err.Diagnostic);
        }

        return Ok();
    }

    public record LockRequestData
    {
        public Hrib Id { get; set; } = Hrib.Empty;
    }
}
