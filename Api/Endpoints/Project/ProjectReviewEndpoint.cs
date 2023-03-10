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
[Route("project-review")]
[Authorize]
public class ProjectReviewEndpoint : EndpointBaseAsync
    .WithRequest<ProjectReviewCreationDto>
    .WithoutResult
{
    private readonly IProjectService projects;

    public ProjectReviewEndpoint(IProjectService projects)
    {
        this.projects = projects;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task HandleAsync(
        ProjectReviewCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        await projects.Review(dto, cancellationToken);
    }
}
