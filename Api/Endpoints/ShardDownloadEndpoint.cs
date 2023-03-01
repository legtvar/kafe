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
[Route("shard-download/{id}/{variant?}")]
[Authorize]
public class ShardDownloadEndpoint : EndpointBaseAsync
    .WithRequest<ShardDownloadEndpoint.RequestData>
    .WithActionResult
{
    private readonly IShardService shards;

    public ShardDownloadEndpoint(IShardService shards)
    {
        this.shards = shards;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Shard })]
    [Produces("video/mp4", "video/x-matroska", "application/octet-stream", Type = typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] RequestData data,
        CancellationToken cancellationToken = default)
    {
        var mediaType = await shards.GetShardVariantMediaType(data.Id, data.Variant, cancellationToken);
        if (mediaType is null)
        {
            return NotFound();
        }

        var stream = await shards.OpenStream(data.Id, data.Variant, cancellationToken);
        return File(stream, mediaType.MimeType, $"{data.Id}.{mediaType.Variant}{mediaType.FileExtension}", true);
    }

    public record RequestData(
        [FromRoute] string Id,
        [FromRoute] string? Variant);
}
