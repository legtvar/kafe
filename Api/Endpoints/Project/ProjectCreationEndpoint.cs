﻿using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using System.Linq;
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
    private readonly ProjectService projectService;
    private readonly UserProvider userProvider;
    private readonly IAuthorizationService authorizationService;

    public ProjectCreationEndpoint(
        ProjectService projectService,
        UserProvider userProvider,
        IAuthorizationService authorizationService)
    {
        this.projectService = projectService;
        this.userProvider = userProvider;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Project])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, request.ProjectGroupId, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var project = await projectService.Upsert(
            project: ProjectInfo.Create(
                projectGroupId: request.ProjectGroupId,
                name: request.Name) with
            {
                Description = request.Description,
                Genre = request.Genre,
                Authors = request.Cast
                    .Select(c => new ProjectAuthorInfo(c.Id.ToString(), ProjectAuthorKind.Cast, c.Roles))
                    .Concat(request.Crew
                        .Select(c => new ProjectAuthorInfo(c.Id.ToString(), ProjectAuthorKind.Crew, c.Roles)))
                    .ToImmutableArray()
            },
            ownerId: userProvider.AccountId,
            existingEntityHandling: ExistingEntityHandling.Insert,
            token: cancellationToken
        );
        if (project.HasErrors)
        {
            return this.KafeErrResult(project);
        }

        return Ok((Hrib)project.Value.Id);
    }
}
