using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1.0")]
[Route("video/{shardId}/{variant}")]
[Authorize]
public class VideoShardDownloadEndpoint : EndpointBaseSync
    .WithRequest<VideoShardDownloadEndpoint.RequestData>
    .WithActionResult
{
    private readonly IArtifactService artifacts;

    public VideoShardDownloadEndpoint(IArtifactService artifacts)
    {
        this.artifacts = artifacts;
    }

    [HttpGet]
    public override ActionResult Handle([FromRoute] RequestData data)
    {
        var (stream, mime) = artifacts.OpenVideoShard(data.ShardId, data.Variant);
        return File(stream, mime);
    }

    public record RequestData(
        [FromRoute] string ShardId,
        [FromRoute] string Variant);
}
