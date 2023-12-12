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
    private readonly IAuthorizationService authorizationService;

    public ProjectValidationEndpoint(
        ProjectService projectService,
        IAuthorizationService authorizationService)
    {
        this.projectService = projectService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<ProjectValidationDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // NB: Only project owners (i.e. account with Write on the project) can validate projects since only they
        //     can make changes to them anyway.
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
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
