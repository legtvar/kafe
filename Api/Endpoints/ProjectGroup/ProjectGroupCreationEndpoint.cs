﻿using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group")]
[Authorize(EndpointPolicy.Append)]
public class ProjectGroupCreationEndpoint : EndpointBaseAsync
    .WithRequest<ProjectGroupCreationDto>
    .WithActionResult<Hrib>
{
    private readonly ProjectGroupService projectGroupService;

    public ProjectGroupCreationEndpoint(
        ProjectGroupService projectGroupService,
        IAuthorizationService authorizationService)
    {
        this.projectGroupService = projectGroupService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectGroupCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var group = await projectGroupService.Create(
            name: dto.Name,
            description: dto.Description,
            deadline: dto.Deadline,
            token: cancellationToken);
        return Ok(group);
    }
}
