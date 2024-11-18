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
using Kafe.Common;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group/unlock")]
public class ProjectGroupUnlockEndpoint : EndpointBaseAsync
    .WithRequest<ProjectGroupUnlockEndpoint.RequestData>
    .WithActionResult
{
    private readonly ProjectGroupService projectGroupService;
    private readonly ProjectService projectService;
    private readonly IAuthorizationService authorizationService;

    public ProjectGroupUnlockEndpoint(
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
        RequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, requestData.Id, EndpointPolicy.Administer);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var projects = await projectService.List(new(ProjectGroupId: requestData.Id), token: cancellationToken);
        var errors = ImmutableArray.CreateBuilder<string>();
        foreach(var project in projects)
        {
            if (project.IsLocked)
            {
                var result = await projectService.Unlock(project.Id, cancellationToken);
                if (result.HasErrors)
                {
                    errors.Add(project.Id);
                }
            }
        }
        if (errors.Any())
        {
            return new Err<bool>(new Error($"Some projects could not be unlocked: {string.Join(", ", errors)}"))
                .ToActionResult();
        }
        return Ok();
    }
    
    public record RequestData
    {
        public Hrib Id { get; set; } = Hrib.Empty;
    }
}
