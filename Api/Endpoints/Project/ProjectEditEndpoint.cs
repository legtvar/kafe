using System;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core;
using Kafe.Legacy.Corrections;

namespace Kafe.Api.Endpoints.Project;

[ApiVersion("1")]
[Route("project")]
[Authorize]
[Obsolete("This endpoint is part of the old artifact abstraction and will soon be replaced.")]
public class ProjectEditEndpoint(
    ProjectService projectService,
    ArtifactService artifactService,
    KafeObjectFactory objectFactory,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ProjectEditDto>
    .WithActionResult<Hrib>
{
    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.Project])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectEditDto request,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, request.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var existingErr = await projectService.Load(request.Id, token: ct);
        if (existingErr.HasError)
        {
            return this.KafeErrorResult(existingErr.Diagnostic);
        }

        var existing = existingErr.Value;

        if (request.IsLocked != null)
        {
            var lockedAuth = await authorizationService.AuthorizeAsync(User, request.Id, EndpointPolicy.Administer);
            if (!lockedAuth.Succeeded)
            {
                return Unauthorized();
            }
        }

        if (existing.ArtifactId is null)
        {
            // NB: This simply should not happen with these old endpoints.
            return BadRequest("The project has no inner artifact. This is likely a bug.");
        }

        var artifactErr = await artifactService.Load(existing.ArtifactId, ct);
        if (artifactErr.HasError)
        {
            return this.KafeErrorResult(artifactErr.Diagnostic);
        }

        var artifact = artifactErr.Value;
        if (request.Crew.HasValue)
        {
            artifact = artifact with
            {
                Properties = artifact.Properties.SetItem(
                    LegacyBlueprintsCorrection.CrewProp,
                    objectFactory.Wrap(
                        request.Crew.Value.Select(c => new AuthorReference(AuthorId: c.Id, Name: null, Roles: c.Roles))
                    )
                )
            };
        }

        if (request.Cast.HasValue)
        {
            artifact = artifact with
            {
                Properties = artifact.Properties.SetItem(
                    LegacyBlueprintsCorrection.CastProp,
                    objectFactory.Wrap(
                        request.Cast.Value.Select(c => new AuthorReference(AuthorId: c.Id, Name: null, Roles: c.Roles))
                    )
                )
            };
        }

        object? WrapShardReferences(string blueprintSlot)
        {
            var ids = request.Artifacts?.Where(a => a.BlueprintSlot == blueprintSlot)
                .Select(a => a.Id)
                .ToImmutableArray() ?? [];
            return ids.Length switch
            {
                0 => null,
                1 => new ShardReference(ids[0]),
                _ => ids.Select(i => new ShardReference(i)).ToImmutableArray()
            };
        }

        artifact = artifact with
        {
            Properties = artifact.Properties.SetItems(
                objectFactory.WrapProperties(
                    (LegacyBlueprintsCorrection.NameProp, request.Name),
                    (LegacyBlueprintsCorrection.GenreProp, request.Genre),
                    (LegacyBlueprintsCorrection.DescriptionProp, request.Description),
                    (LegacyBlueprintsCorrection.FilmProp, WrapShardReferences(Const.FilmBlueprintSlot)),
                    (
                        LegacyBlueprintsCorrection.VideoAnnotationProp,
                        WrapShardReferences(Const.VideoAnnotationBlueprintSlot)
                    ),
                    (LegacyBlueprintsCorrection.CoverPhotosProp, WrapShardReferences(Const.CoverPhotoBlueprintSlot)),
                    (LegacyBlueprintsCorrection.BtsPhotosProp, WrapShardReferences("bts-photo"))
                )
            )
        };

        artifactErr = await artifactService.Upsert(artifact, ct);
        if (artifactErr.HasError)
        {
            return this.KafeErrorResult(artifactErr.Diagnostic);
        }

        var @new = existing with
        {
            IsLocked = request.IsLocked ?? existing.IsLocked
        };
        var result = await projectService.Upsert(
            project: @new,
            existingEntityHandling: ExistingEntityHandling.Update,
            token: ct
        );
        if (result.HasError)
        {
            return this.KafeErrResult(result);
        }

        return Ok((Hrib)@new.Id);
    }
}
