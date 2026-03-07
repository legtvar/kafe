using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;
using System.Linq;
using System.Collections.Immutable;

namespace Kafe.Api.Endpoints.ProjectGroup;

[ApiVersion("1")]
[Route("project-group/{id}")]
public class ProjectGroupDetailEndpoint(
    ProjectGroupService projectGroupService,
    ProjectService projectService,
    ArtifactService artifactService,
    IAuthorizationService authorizationService,
    UserProvider userProvider,
    EntityService entityService
)
    : EndpointBaseAsync
        .WithRequest<string>
        .WithActionResult<ProjectGroupDetailDto>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.ProjectGroup])]
    [ProducesResponseType(typeof(ProjectGroupDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectGroupDetailDto>> HandleAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var projectGroupErr = await projectGroupService.Load(id, ct);
        if (projectGroupErr.HasError)
        {
            return this.KafeErrResult(projectGroupErr);
        }

        var projectGroup = projectGroupErr.Value;
        var dto = TransferMaps.ToProjectGroupDetailDto(projectGroup);

        if (auth.Succeeded)
        {
            var projects = await projectService.List(
                new ProjectService.ProjectFilter(
                    ProjectGroupId: projectGroup.Id,
                    AccessingAccountId: userProvider.AccountId
                ),
                token: ct
            );
            var projectPerms = await entityService.GetPermissions(
                [..projects.Select(p => (Hrib)p.Id)],
                userProvider.AccountId,
                ct
            );
            var artifactsErr = await artifactService.LoadMany(
                [..projects.Select(p => p.ArtifactId).OfType<string>()],
                ct
            );
            if (artifactsErr.HasError)
            {
                return this.KafeErrResult(artifactsErr);
            }

            var artifacts = artifactsErr.Value.ToImmutableDictionary(a => a.Id);
            var preferredCulture = userProvider.Account?.PreferredCulture ?? Const.InvariantCultureCode;
            dto = dto with
            {
                Projects =
                [
                    ..projects.Zip(projectPerms)
                        .Select(p => TransferMaps.ToProjectListDto(
                                data: p.First,
                                artifact: p.First.ArtifactId is not null
                                    ? artifacts[p.First.ArtifactId]
                                    : null,
                                userPermission: p.Second
                            )
                        )
                        .OrderBy(p => p.Name[preferredCulture])
                ]
            };
        }

        return Ok(dto);
    }
}
