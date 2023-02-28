using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
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
    [SwaggerOperation(Tags = new[] { SwaggerTags.VideoShard })]
    [Produces("application/octet-stream", Type = typeof(FileStreamResult))]
    public override ActionResult Handle([FromRoute] RequestData data)
    {
        var (stream, mime) = artifacts.OpenVideoShard(data.ShardId, data.Variant);
        var extension = mime == Const.MatroskaMimeType ? Const.MatroskaFileExtension : Const.Mp4FileExtension;
        return File(stream, "application/octet-stream", $"{data.ShardId}{extension}", true);
    }

    public record RequestData(
        [FromRoute] string ShardId,
        [FromRoute] string Variant);
}
