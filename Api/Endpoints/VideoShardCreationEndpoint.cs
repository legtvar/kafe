using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1.0")]
[Route("artifact/{artifactId}/video")]
[Authorize]
public class VideoShardCreationEndpoint : EndpointBaseAsync
    .WithRequest<VideoShardCreationEndpoint.RequestData>
    .WithActionResult<Hrib>
{
    private readonly IArtifactService artifacts;

    public VideoShardCreationEndpoint(IArtifactService artifacts)
    {
        this.artifacts = artifacts;
    }

    [HttpPost]
    [RequestSizeLimit(Const.VideoShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.VideoShardSizeLimit)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        [FromRoute]RequestData request,
        CancellationToken cancellationToken = default)
    {
        using var stream = request.File.OpenReadStream();
        var id = await artifacts.AddVideoShard(
            request.ArtifactId,
            request.File.ContentType,
            stream,
            cancellationToken);
        if (id is null)
        {
            return BadRequest();
        }

        return Ok(id);
    }

    public record RequestData(
        [FromRoute(Name = "artifactId")] string ArtifactId,
        [FromBody] IFormFile File);
}
