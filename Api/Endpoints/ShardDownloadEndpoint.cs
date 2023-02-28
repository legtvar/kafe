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
[Route("shard/{id}/{variant}")]
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
    [Produces("application/octet-stream", Type = typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] RequestData data,
        CancellationToken cancellationToken = default)
    {
        var stream = await shards.OpenStream(data.Id, data.Variant, cancellationToken);
        return File(stream, "application/octet-stream", $"{data.Id}", true);
    }

    public record RequestData(
        [FromRoute] string Id,
        [FromRoute] string Variant);
}
