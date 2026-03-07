using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Mvc;
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
public class ProjectGroupListEndpoint(
    ProjectGroupService projectGroupService,
    UserProvider userProvider
) : EndpointBaseAsync
    .WithRequest<ProjectGroupListEndpoint.RequestData>
    .WithActionResult<ImmutableArray<ProjectGroupListDto>>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    public override async Task<ActionResult<ImmutableArray<ProjectGroupListDto>>> HandleAsync(
        RequestData request,
        CancellationToken cancellationToken = default
    )
    {
        var filter = new ProjectGroupService.ProjectGroupFilter(
            AccessingAccountId: userProvider.AccountId,
            OrganizationId: request.OrganizationId
        );

        var groups = await projectGroupService.List(filter, request.Sort, cancellationToken);
        return Ok(groups.Select(TransferMaps.ToProjectGroupListDto));
    }

    public record RequestData
    {
        [FromQuery(Name = "organization")]
        public string? OrganizationId { get; set; }

        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name.iv";
    }
}
