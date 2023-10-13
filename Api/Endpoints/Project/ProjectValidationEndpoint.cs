using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
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
[Route("project-validation/{id}")]
[Authorize]
public class ProjectValidationEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectValidationDto>
{
    private readonly ProjectService projectService;

    public ProjectValidationEndpoint(ProjectService projectService)
    {
        this.projectService = projectService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<ProjectValidationDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var report = await projectService.Validate(id, cancellationToken);
        return Ok(new ProjectValidationDto(
            ProjectId: id,
            ValidatedOn: report.ValidatedOn,
            Diagnostics: report.Diagnostics.Select(d => new ProjectDiagnosticDto(
                Kind: d.Kind,
                Message: d.Message,
                ValidationStage: d.ValidationStage
            ))
            .ToImmutableArray()
        ));
    }
}
