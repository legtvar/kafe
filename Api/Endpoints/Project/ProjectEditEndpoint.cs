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
public class ProjectEditEndpoint : EndpointBaseAsync
    .WithRequest<ProjectEditDto>
    .WithoutResult
{
    private readonly IProjectService projects;

    public ProjectEditEndpoint(IProjectService projects)
    {
        this.projects = projects;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task HandleAsync(
        ProjectEditDto request,
        CancellationToken cancellationToken = default)
    {
        await projects.Edit(request, cancellationToken);
    }
}
