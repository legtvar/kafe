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

    public ShardCreationEndpoint(ShardService shardService)
    {
        this.shardService = shardService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [RequestSizeLimit(Const.ShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.ShardSizeLimit)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        [FromForm] RequestData request,
        CancellationToken cancellationToken = default)
    {
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

        return Ok(id);
    }

    public record RequestData(
        [FromForm] ShardKind Kind,
        [FromForm] string ArtifactId,
        IFormFile File);
}
