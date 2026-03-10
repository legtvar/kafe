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
public class ProjectListEndpoint(
    ProjectService projectService,
    EntityService entityService,
    ArtifactService artifactService,
    UserProvider userProvider
) : EndpointBaseAsync
    .WithRequest<ProjectListEndpoint.RequestData>
    .WithActionResult<ImmutableArray<ProjectListDto>>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Project])]
    public override async Task<ActionResult<ImmutableArray<ProjectListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken ct = default
    )
    {
        var filter = new ProjectService.ProjectFilter(
            AccessingAccountId: userProvider.AccountId,
            OrganizationId: requestData.OrganizationId,
            ProjectGroupId: requestData.ProjectGroupId
        );
        var projects = (await projectService.List(filter, requestData.Sort, ct)).Where(p => p.ArtifactId is not null);
        var perms = await entityService.GetPermissions(
            [.. projects.Select(p => (Hrib)p.Id)],
            userProvider.AccountId,
            ct
        );
        var artifactsErr = await artifactService.LoadMany([.. projects.Select(p => p.ArtifactId!)], ct);
        if (artifactsErr.HasError)
        {
            return this.KafeErrorResult(artifactsErr.Diagnostic);
        }

        var artifacts = artifactsErr.Value;
        return Ok(projects.Zip(artifacts).Zip(perms)
            .Select((p) => TransferMaps.ToProjectListDto(p.First.First, p.First.Second, p.Second))
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
