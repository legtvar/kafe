using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard-download/{id}")]
[Route("shard-download/{id}/{variant}")]
public class ShardDownloadEndpoint : EndpointBaseAsync
    .WithRequest<ShardDownloadEndpoint.RequestData>
    .WithActionResult
{
    private readonly ShardService shardService;
    private readonly IAuthorizationService authorizationService;

    public ShardDownloadEndpoint(
        ShardService shardService,
        IAuthorizationService authorizationService)
    {
        this.shardService = shardService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [Produces(typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] RequestData data,
        CancellationToken cancellationToken = default)
    {
        var detail = await shardService.Load(data.Id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        var auth = await authorizationService.AuthorizeAsync(User, (Hrib)detail.ArtifactId, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var mediaType = await shardService.GetShardVariantMediaType(data.Id, data.Variant, cancellationToken);
        if (mediaType is null || mediaType.MimeType is null)
        {
            return NotFound();
        }

        var stream = await shardService.OpenStream(data.Id, data.Variant, cancellationToken);
        return File(stream, mediaType.MimeType, $"{data.Id}.{mediaType.Variant}{mediaType.FileExtension}", true);
    }

    public record RequestData(
        [FromRoute] string Id,
        string? Variant);
}
