using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group/lock")]
public class ProjectGroupLockEndpoint : EndpointBaseAsync
    .WithRequest<ProjectGroupLockEndpoint.LockRequestData>
    .WithActionResult
{
    private readonly ProjectGroupService projectGroupService;
    private readonly ProjectService projectService;
    private readonly IAuthorizationService authorizationService;

    public ProjectGroupLockEndpoint(
        ProjectGroupService projectGroupService,
        ProjectService projectService,
        IAuthorizationService authorizationService)
    {
        this.projectGroupService = projectGroupService;
        this.projectService = projectService;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    public override async Task<ActionResult> HandleAsync(
        [FromBody] LockRequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, requestData.Id, EndpointPolicy.Administer);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var projects = await projectService.List(new(ProjectGroupId: requestData.Id), token: cancellationToken);
        var errors = ImmutableArray.CreateBuilder<string>();
        foreach (var project in projects)
        {
            if (!project.IsLocked)
            {
                var result = await projectService.Lock(project.Id, cancellationToken);
                if (result.Diagnostic is not null)
                {
                    errors.Add(project.Id);
                }
            }
        }
        if (errors.Any())
        {
            return this.KafeErrorResult(new Diagnostic($"Some projects could not be locked: {string.Join(", ", errors)}"));
        }

        return Ok();
    }

    public record LockRequestData
    {
        public Hrib Id { get; set; } = Hrib.Empty;
    }
}
