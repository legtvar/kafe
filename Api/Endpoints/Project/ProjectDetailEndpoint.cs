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
using Kafe.Core.Diagnostics;
using Kafe.Legacy.Corrections;
using Kafe.Media;

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
    [SwaggerOperation(Tags = [EndpointArea.Project])]
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

        var compatArtifacts = ImmutableArray.CreateBuilder<ProjectArtifactDto>();

        async Task<Err<bool>> AddCompatArtifacts(string propertyKey, string blueprintSlot)
        {
            var compatArtifactErr = await GetCompatArtifacts(
                artifact,
                propertyKey, blueprintSlot, ct
            );
            if (compatArtifactErr is { HasError: true, Diagnostic.Payload: not PropertyNotFoundDiagnostic })
            {
                return compatArtifactErr.Diagnostic;
            }
            else if (compatArtifactErr.HasValue)
            {
                compatArtifacts.AddRange(compatArtifactErr.Value);
            }

            return true;
        }

        Task<Err<bool>>[] compatArtifactTasks =
        [
            AddCompatArtifacts(LegacyBlueprintsCorrection.FilmProp, Const.FilmBlueprintSlot),
            AddCompatArtifacts(LegacyBlueprintsCorrection.VideoAnnotationProp, Const.VideoAnnotationBlueprintSlot),
            AddCompatArtifacts(LegacyBlueprintsCorrection.CoverPhotosProp, Const.CoverPhotoBlueprintSlot),
            AddCompatArtifacts(LegacyBlueprintsCorrection.BtsPhotosProp, "bts-photo")
        ];

        foreach (var compatArtifactTask in compatArtifactTasks)
        {
            var err = await compatArtifactTask;
            if (err.HasError)
            {
                return this.KafeErrorResult(err.Diagnostic);
            }
        }

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
            Artifacts = compatArtifacts.ToImmutable(),
            Cast =
            [
                .. cast.Zip(castAuthors.Value).Select(p => new ProjectAuthorDto(
                        Id: p.Second.Id,
                        Name: p.Second.Name,
                        Roles: p.First.Roles
                    )
                )
            ],
            Crew =
            [
                .. crew.Zip(crewAuthors.Value).Select(p => new ProjectAuthorDto(
                        Id: p.Second.Id,
                        Name: p.Second.Name,
                        Roles: p.First.Roles
                    )
                )
            ],
        };

        return Ok(dto);
    }

    private async Task<Err<ImmutableArray<ProjectArtifactDto>>> GetCompatArtifacts(
        ArtifactInfo artifact,
        string propertyKey,
        string blueprintSlot,
        CancellationToken ct = default
    )
    {
        var prop = artifact.GetProperty(propertyKey);
        if (prop is not null)
        {
            ImmutableArray<ShardInfo> shards;
            if (prop is ShardReference shardRef)
            {
                var shardErr = await shardService.Load(shardRef.ShardId, ct);
                if (shardErr.HasError)
                {
                    return shardErr.Diagnostic;
                }

                shards = [shardErr.Value];
            }
            else if (prop is ImmutableArray<ShardReference> shardRefArray)
            {
                var shardsErr = await shardService.LoadMany([..shardRefArray.Select(s => s.ShardId)], ct);
                if (shardsErr.HasError)
                {
                    return shardsErr.Diagnostic;
                }

                shards = shardsErr.Value;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Expected a shard-ref of shard-ref[] property, got '{prop.GetType()}'."
                );
            }

            var compatShards = ImmutableArray.CreateBuilder<ProjectArtifactDto>();
            foreach (var shard in shards)
            {
                var mockShards = ImmutableArray.CreateBuilder<ShardListDto>();
                mockShards.Add(TransferMaps.ToShardListDto(shard));
                foreach (var link in shard.LinksByType.GetValueOrDefault(typeof(SubtitlesShardLink)))
                {
                    mockShards.Add(
                        new ShardListDto(link.DestinationId, ShardKind.Subtitles, [Const.OriginalShardVariant])
                    );
                }

                compatShards.Add(
                    new ProjectArtifactDto(
                        Id: shard.Id,
                        Name: shard.Name,
                        AddedOn: null,
                        BlueprintSlot: blueprintSlot,
                        Shards: mockShards.ToImmutable()
                    )
                );
            }

            return compatShards.ToImmutable();
        }

        return Err.Fail(new PropertyNotFoundDiagnostic(artifact.Id, propertyKey));
    }
}
