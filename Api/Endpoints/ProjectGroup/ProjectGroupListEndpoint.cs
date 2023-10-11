using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using System.Collections.Immutable;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-groups")]
public class ProjectGroupListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<ProjectGroupListDto>>
{
    private readonly ProjectGroupService projectGroupService;

    public ProjectGroupListEndpoint(ProjectGroupService projectGroupService)
    {
        this.projectGroupService = projectGroupService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.ProjectGroup })]
    public override async Task<ActionResult<ImmutableArray<ProjectGroupListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await projectGroupService.List(cancellationToken);
        return Ok(groups.Select(TransferMaps.ToProjectGroupListDto));
    }
}
