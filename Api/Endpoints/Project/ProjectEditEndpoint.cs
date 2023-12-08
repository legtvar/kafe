using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
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
[Route("project")]
[Authorize]
public class ProjectEditEndpoint : EndpointBaseAsync
    .WithRequest<ProjectEditDto>
    .WithActionResult
{
    private readonly ProjectService projects;
    private readonly IAuthorizationService authorizationService;

    public ProjectEditEndpoint(
        ProjectService projects,
        IAuthorizationService authorizationService)
    {
        this.projects = projects;
        this.authorizationService = authorizationService;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult> HandleAsync(
        ProjectEditDto request,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, request.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var authors = ImmutableArray<ProjectAuthorInfo>.Empty;
        if (request.Crew.HasValue)
        {
            authors = authors.AddRange(request.Crew.Value.Select(c => new ProjectAuthorInfo(
                Id: c.Id,
                Kind: ProjectAuthorKind.Crew,
                Roles: c.Roles
            )));
        }
        if (request.Cast.HasValue)
        {
            authors = authors.AddRange(request.Cast.Value.Select(c => new ProjectAuthorInfo(
                Id: c.Id,
                Kind: ProjectAuthorKind.Cast,
                Roles: c.Roles
            )));
        }

        var @old = await projects.Load(request.Id, token: cancellationToken);
        if (@old is null)
        {
            return NotFound();
        }

        var @new = @old with
        {
            Name = request.Name ?? @old.Name,
            Genre = request.Genre ?? @old.Genre,
            Description = request.Description ?? @old.Description,
            Authors = (request.Crew.HasValue || request.Cast.HasValue)
                ? authors
                : @old.Authors,
            Artifacts = request.Artifacts.HasValue
                ? request.Artifacts.Value.Select(a => new ProjectArtifactInfo(
                    Id: a.Id,
                    BlueprintSlot: a.BlueprintSlot
                )).ToImmutableArray()
                : @old.Artifacts
        };
        var result = await projects.Edit(@new, cancellationToken);
        if (result.HasErrors)
        {
            return ValidationProblem(title: result.Errors.FirstOrDefault());
        }
        
        return Ok();
    }
}
