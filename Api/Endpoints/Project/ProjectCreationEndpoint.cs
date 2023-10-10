using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project")]
[Authorize]
public class ProjectCreationEndpoint : EndpointBaseAsync
    .WithRequest<ProjectCreationDto>
    .WithActionResult<Hrib>
{
    private readonly IProjectService projects;
    private readonly IUserProvider userProvider;

    public ProjectCreationEndpoint(
        IProjectService projects,
        IUserProvider userProvider)
    {
        this.projects = projects;
        this.userProvider = userProvider;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.Create(request, userProvider.Account?.Id, cancellationToken);
        return Ok(project.Id);
    }
}
