﻿using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, request.ProjectGroupId, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var project = await projectService.Create(
            projectGroupId: request.ProjectGroupId.ToString(),
            name: request.Name,
            description: request.Description,
            genre: request.Genre,
            ownerId: userProvider.Account?.Id,
            token: cancellationToken);

        return Ok(project.Id);
    }
}
