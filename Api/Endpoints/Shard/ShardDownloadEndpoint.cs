using Ardalis.ApiEndpoints;
using Asp.Versioning;
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
public class ShardDownloadEndpoint(
    ShardService shardService,
    FileExtensionMimeMap mimeMap,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<ShardDownloadEndpoint.RequestData>
    .WithActionResult
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Shard])]
    [Produces(typeof(FileStreamResult))]
    public override async Task<ActionResult> HandleAsync(
        RequestData data,
        CancellationToken ct = default
    )
    {
        var shardErr = await shardService.Load(data.Id, ct);
        if (shardErr.HasError)
        {
            return this.KafeErrResult(shardErr);
        }

        var shard = shardErr.Value;
        var auth = await authorizationService.AuthorizeAsync(User, (Hrib)shard.Id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var extension = mimeMap.GetFirstFileExtensionFor(shard.MimeType);
        var streamErr = await shardService.OpenStream(data.Id, data.Variant, ct);
        if (streamErr.HasError)
        {
            return this.KafeErrResult(streamErr);
        }

        return File(
            fileStream: streamErr.Value,
            contentType: shard.MimeType,
            fileDownloadName: $"{shard.Id}{extension}",
            enableRangeProcessing: true
        );
    }

    public record RequestData
    {
        [FromRoute]
        public string Id { get; set; } = string.Empty;

        [FromRoute]
        public string? Variant { get; set; }
    }
}
