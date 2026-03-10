using System;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Artifact;

[ApiVersion("1")]
[Route("artifact")]
[Authorize]
[Obsolete("This endpoint is part of the old artifact abstraction and will soon be replaced.")]
public class ArtifactCreationEndpoint(
    ArtifactService artifacts,
    KafeObjectFactory objectFactory,
    IAuthorizationService authorization
) : EndpointBaseAsync
    .WithRequest<ArtifactCreationDto>
    .WithActionResult<Hrib>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Artifact])]
    [ProducesResponseType(typeof(Hrib), 200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ArtifactCreationDto request,
        CancellationToken ct = default
    )
    {
        var auth = await authorization.AuthorizeAsync(User, request.ContainingProject, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var artifactErr = await artifacts.Upsert(
            ArtifactInfo.Create() with
            {
                AddedOn = request.AddedOn ?? default,
                CreationMethod = CreationMethod.Api,
                Properties = objectFactory.WrapProperties((Const.ArtifactNameProperty, request.Name))
            },
            ct
        );

        if (artifactErr.HasError)
        {
            return this.KafeErrResult(artifactErr);
        }

        if (request.ContainingProject is not null)
        {
            throw new NotSupportedException(
                "This endpoint can no longer be used to append a newly created artifact onto a project. "
                + "Each project has its own, automatically created artifact. "
                + "To nest artifacts, use the new properties interface."
            );
        }

        return Ok((Hrib)artifactErr.Value.Id);
    }
}
