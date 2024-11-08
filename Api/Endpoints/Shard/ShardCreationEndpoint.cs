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

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard")]
[Authorize]
public class ShardCreationEndpoint : EndpointBaseAsync
    .WithRequest<ShardCreationEndpoint.RequestData>
    .WithActionResult<Hrib>
{
    private readonly ShardService shardService;
    private readonly ArtifactService artifactService;
    private readonly IAuthorizationService authorizationService;

    public ShardCreationEndpoint(
        ShardService shardService,
        ArtifactService artifactService,
        IAuthorizationService authorizationService)
    {
        this.shardService = shardService;
        this.artifactService = artifactService;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [RequestSizeLimit(Const.ShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.ShardSizeLimit)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        [FromForm] RequestData request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await artifactService.LoadDetail(request.ArtifactId, cancellationToken);
        if (artifact is null)
        {
            return NotFound();
        }

        var auth = await authorizationService.AuthorizeAsync(User, artifact, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        using var stream = request.File.OpenReadStream();
        var id = await shardService.Create(
            kind: request.Kind,
            artifactId: request.ArtifactId,
            stream: stream,
            mimeType: request.File.ContentType,
            token: cancellationToken);
        if (id is null)
        {
            return BadRequest();
        }

        return Ok(id.ToString());
    }

    public record RequestData(
        [FromForm] ShardKind Kind,
        [FromForm] string ArtifactId,
        IFormFile File);
}
