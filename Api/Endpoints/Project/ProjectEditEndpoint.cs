﻿using Ardalis.ApiEndpoints;
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
    private readonly ProjectService projectService;
    private readonly IAuthorizationService authorizationService;

    public ProjectEditEndpoint(
        ProjectService projectService,
        IAuthorizationService authorizationService)
    {
        this.projectService = projectService;
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

        var @old = await projectService.Load(request.Id, token: cancellationToken);
        if (@old is null)
        {
            return NotFound();
        }

        var authors = old.Authors;
        if (request.Crew.HasValue)
        {
            authors = authors
                .RemoveAll(a => a.Kind == ProjectAuthorKind.Crew)
                .AddRange(request.Crew.Value.Select(c => new ProjectAuthorInfo(
                    Id: c.Id.Value,
                    Kind: ProjectAuthorKind.Crew,
                    Roles: c.Roles
                ))
            );
        }

        if (request.Cast.HasValue)
        {
            authors = authors
                .RemoveAll(a => a.Kind == ProjectAuthorKind.Cast)
                .AddRange(request.Cast.Value.Select(c => new ProjectAuthorInfo(
                    Id: c.Id.Value,
                    Kind: ProjectAuthorKind.Cast,
                    Roles: c.Roles
                ))
            );
        }

        var @new = @old with
        {
            Name = LocalizedString.Override(@old.Name, request.Name),
            Genre = LocalizedString.Override(@old.Genre, request.Genre),
            Description = LocalizedString.Override(@old.Description, request.Description),
            Authors = authors,
            Artifacts = request.Artifacts.HasValue
                ? request.Artifacts.Value.Select(a => new ProjectArtifactInfo(
                    Id: a.Id.ToString(),
                    BlueprintSlot: a.BlueprintSlot
                )).ToImmutableArray()
                : @old.Artifacts
        };
        var result = await projectService.Edit(@new, cancellationToken);
        if (result.HasErrors)
        {
            return ValidationProblem(title: result.Errors.FirstOrDefault());
        }

        return Ok();
    }
}
