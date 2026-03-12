using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core;
using Kafe.Data.Aggregates;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard")]
[Authorize]
[Obsolete("This endpoint is part of the old artifact abstraction and will soon be replaced.")]
public class ShardCreationEndpoint(
    ShardService shardService,
    ArtifactService artifactService,
    KafeObjectFactory objectFactory,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ShardCreationEndpoint.RequestData>
    .WithActionResult<Hrib>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Shard])]
    [RequestSizeLimit(Const.ShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.ShardSizeLimit)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        [FromForm] RequestData request,
        CancellationToken ct = default
    )
    {
        if (request.ArtifactId is null != request.ArtifactProperty is null)
        {
            return BadRequest(
                "If a shard is to be appended to an artifact, "
                + "both the ArtifactId and ArtifactProperty must be provided."
            );
        }

        ArtifactInfo? artifact = null;

        if (request.ArtifactId is not null)
        {
            var artifactErr = await artifactService.Load(request.ArtifactId, ct);
            if (artifactErr.HasError)
            {
                return this.KafeErrorResult(artifactErr.Diagnostic);
            }

            artifact = artifactErr.Value;
            var auth = await authorizationService.AuthorizeAsync(User, artifact, EndpointPolicy.Write);
            if (!auth.Succeeded)
            {
                return Unauthorized();
            }
        }

        await using var stream = request.File.OpenReadStream();
        var shardErr = await shardService.Create(
            shardType: ShardCompat.ToType(request.Kind),
            stream: stream,
            uploadFilename: request.File.FileName,
            mimeType: request.File.ContentType,
            token: ct
        );
        if (shardErr.HasError)
        {
            return this.KafeErrResult(shardErr);
        }

        var shard = shardErr.Value;
        if (artifact is not null)
        {
            artifact = artifact with
            {
                Properties = artifact.Properties.SetItem(
                    request.ArtifactProperty!,
                    objectFactory.Wrap(new ShardReference(shard.Id))
                )
            };
            var artifactErr = await artifactService.Upsert(artifact, ct);
            if (artifactErr.HasError)
            {
                // TODO: We should return both the shard id and the error, since the shard creation succeeded.
                return this.KafeErrResult(artifactErr.Select(_ => shard.Id));
            }
        }

        return Ok(shard.Id);
    }

    public record RequestData(
        [FromForm]
        ShardKind Kind,
        [FromForm]
        string? ArtifactId,
        [FromForm]
        string? ArtifactProperty,
        IFormFile File
    );
}
