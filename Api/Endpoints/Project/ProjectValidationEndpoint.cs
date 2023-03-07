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
[Route("project-validation/{id}")]
[Authorize]
public class ProjectValidationEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectValidationDto>
{
    private readonly IProjectService projects;

    public ProjectValidationEndpoint(IProjectService projects)
    {
        this.projects = projects;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<ProjectValidationDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await projects.Validate(id, cancellationToken);
    }
}
