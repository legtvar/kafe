using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;
using System;
using Kafe.Core;
using Kafe.Legacy.Corrections;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project/{id}")]
[Obsolete("This endpoint is part of the old artifact abstraction and will soon be replaced.")]
public class ProjectDetailEndpoint(
    ProjectService projectService,
    ProjectGroupService projectGroupService,
    EntityService entityService,
    ArtifactService artifactService,
    AuthorService authorService,
    ShardService shardService,
    UserProvider userProvider,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ProjectDetailDto>
{
    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Project })]
    [ProducesResponseType(typeof(ProjectDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ProjectDetailDto>> HandleAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var projectErr = await projectService.Load(id, ct);
        if (projectErr.HasError)
        {
            return this.KafeErrResult(projectErr);
        }

        var project = projectErr.Value;
        var artifactErr = project.ArtifactId is null
            ? ArtifactInfo.Create()
            : await artifactService.Load(project.ArtifactId, ct);
        if (artifactErr.HasError)
        {
            return this.KafeErrResult(artifactErr);
        }

        var artifact = artifactErr.Value;
        var groupErr = await projectGroupService.Load(project.ProjectGroupId, ct);
        if (groupErr.HasError)
        {
            return this.KafeErrResult(groupErr);
        }

        var group = groupErr.Value;
        var userPerms = await entityService.GetPermission(project.Id, userProvider.AccountId, ct);

        var dto = TransferMaps.ToProjectDetailDto(project, artifact, userPerms) with
        {
            ProjectGroupName = group.Name,
            ValidationSettings = ProjectValidationSettings.Merge(
                group.ValidationSettings,
                ProjectValidationSettings.Default
            ),
            // TODO: temporary workaround until artifact blueprints are implemented
            Blueprint = TransferMaps.GetProjectBlueprintDtoByOrgId(group.OrganizationId)
        };

        var shardProps = artifact?.Properties.Where(p => p.Value.Value is ShardReference).ToImmutableArray() ?? [];
        var shardsErr = await shardService.LoadMany(
            [.. shardProps.Select(p => ((ShardReference)p.Value.Value).ShardId)],
            ct
        );
        if (shardsErr.HasError)
        {
            return this.KafeErrorResult(shardsErr.Diagnostic);
        }

        var compatArtifacts = shardProps.Zip(shardsErr.Value).Select(p => new ProjectArtifactDto(
            Id: p.Second.Id,
            Name: p.Second.Name,
            // NB: CreatedAt and AddedOn are not semantically the same, but until we deprecate these old endpoints,
            //     it might do.
            AddedOn: p.Second.CreatedAt,
            BlueprintSlot: p.First.Key,
            // TODO: Currently doesn't return subtitles, because those will be linked through a ShardLink.
            Shards: [TransferMaps.ToShardListDto(p.Second)]
        )).ToImmutableArray();

        var cast = artifact?.GetProperty<ImmutableArray<AuthorReference>>(LegacyBlueprintsCorrection.CastProp) ?? [];
        var castAuthors = await authorService.LoadMany([.. cast.Select(a => a.AuthorId).OfType<Hrib>()], ct);
        if (castAuthors.HasError)
        {
            return this.KafeErrorResult(castAuthors.Diagnostic);
        }

        var crew = artifact?.GetProperty<ImmutableArray<AuthorReference>>(LegacyBlueprintsCorrection.CrewProp) ?? [];
        var crewAuthors = await authorService.LoadMany([.. crew.Select(a => a.AuthorId).OfType<Hrib>()], ct);
        if (crewAuthors.HasError)
        {
            return this.KafeErrorResult(crewAuthors.Diagnostic);
        }

        dto = dto with
        {
            ProjectGroupName = group?.Name ?? Const.UnknownProjectGroup,
            Artifacts = compatArtifacts,
            Cast = [.. cast.Zip(castAuthors.Value).Select(p => new ProjectAuthorDto(
                Id: p.Second.Id,
                Name: p.Second.Name,
                Roles: p.First.Roles
            ))],
            Crew = [.. crew.Zip(crewAuthors.Value).Select(p => new ProjectAuthorDto(
                Id: p.Second.Id,
                Name: p.Second.Name,
                Roles: p.First.Roles
            ))],
        };

        return Ok(dto);
    }
}
