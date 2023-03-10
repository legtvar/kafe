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

    public ProjectCreationEndpoint(IProjectService projects)
    {
        this.projects = projects;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectCreationDto request,
        CancellationToken cancellationToken = default)
    {
        return await projects.Create(request, cancellationToken);
    }
}
