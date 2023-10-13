using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using Kafe.Api.Services;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("projects")]
[Authorize]
public class ProjectListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<ProjectListDto>>
{
    private readonly ProjectService projectService;
    private readonly UserProvider userProvider;

    public ProjectListEndpoint(
        ProjectService projectService,
        UserProvider userProvider)
    {
        this.projectService = projectService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<ImmutableArray<ProjectListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projects =  await projectService.List(new(AccountId: userProvider.User?.Id), cancellationToken);
        return Ok(projects.Select(TransferMaps.ToProjectListDto).ToImmutableArray());
    }
}
