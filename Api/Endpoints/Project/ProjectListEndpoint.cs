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
    .WithRequest<ProjectListEndpoint.RequestData>
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
    [SwaggerOperation(Tags = [EndpointArea.Project])]
    public override async Task<ActionResult<ImmutableArray<ProjectListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var filter = new ProjectService.ProjectFilter(
            AccessingAccountId: userProvider.AccountId,
            OrganizationId: requestData.OrganizationId,
            ProjectGroupId: requestData.ProjectGroupId
        );
        var projects = await projectService.List(filter, requestData.Sort, cancellationToken);
        var perms = await entityService.
            GetPermissions(projects.Select(p => (Hrib)p.Id),
            userProvider.AccountId,
            cancellationToken);
        return Ok(projects.Zip(perms)
            .Select((p) => TransferMaps.ToProjectListDto(p.First, p.Second))
            .ToImmutableArray());
    }

    public record RequestData
    {
        [FromQuery(Name = "organization")]
        public string? OrganizationId { get; set; }

        [FromQuery(Name = "project-group")]
        public string? ProjectGroupId { get; set; }

        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name.iv";
    }
}
