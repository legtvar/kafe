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
public class ProjectCreationEndpoint(
    ProjectService projectService,
    ArtifactService artifactService,
    KafeObjectFactory objectFactory,
    UserProvider userProvider,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ProjectCreationDto>
    .WithActionResult<Hrib>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Project])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ProjectCreationDto request,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, request.ProjectGroupId, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var artifactErr = await artifactService.Upsert(
            artifact: ArtifactInfo.Create() with
            {
                Properties = objectFactory.WrapProperties(
                    (LegacyBlueprintsCorrection.NameProp, request.Name),
                    (LegacyBlueprintsCorrection.DescriptionProp, request.Description),
                    (LegacyBlueprintsCorrection.GenreProp, request.Genre),
                    (LegacyBlueprintsCorrection.CastProp,
                        request.Cast.Select(a => new AuthorReference(AuthorId: a.Id, Name: null, Roles: a.Roles))
                    ),
                    (LegacyBlueprintsCorrection.CrewProp,
                        request.Crew.Select(a => new AuthorReference(AuthorId: a.Id, Name: null, Roles: a.Roles))
                    )
                )
            },
            ct: ct
        );
        if (artifactErr.HasError)
        {
            return this.KafeErrResult(artifactErr);
        }

        var projectErr = await projectService.Upsert(
            project: ProjectInfo.Create(
                    projectGroupId: request.ProjectGroupId
                ) with
                {
                    ArtifactId = artifactErr.Value.Id
                },
            ownerId: userProvider.AccountId,
            existingEntityHandling: ExistingEntityHandling.Insert,
            token: ct
        );
        if (projectErr.HasError)
        {
            return this.KafeErrResult(projectErr);
        }

        return Ok(projectErr.Value.Id);
    }
}
