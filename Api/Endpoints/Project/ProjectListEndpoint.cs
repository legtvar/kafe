using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using Kafe.Api.Services;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("projects")]
public class ProjectListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<ProjectListDto>>
{
    private readonly ProjectService projectService;
    private readonly EntityService entityService;
    private readonly UserProvider userProvider;

    public ProjectListEndpoint(
        ProjectService projectService,
        EntityService entityService,
        UserProvider userProvider)
    {
        this.projectService = projectService;
        this.entityService = entityService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    public override async Task<ActionResult<ImmutableArray<ProjectListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var projects =  await projectService.List(new(AccessingAccountId: userProvider.Account?.Id), cancellationToken);
        var perms = await entityService.
            GetPermissions(projects.Select(p => (Hrib)p.Id),
            userProvider.Account?.Id,
            cancellationToken);
        return Ok(projects.Zip(perms)
            .Select((p) => TransferMaps.ToProjectListDto(p.First, p.Second))
            .ToImmutableArray());
    }
}
