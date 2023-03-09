using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
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
    private readonly IShardService shards;
    private readonly IOptions<StorageOptions> options;

    public ShardCreationEndpoint(
        IShardService shards,
        IOptions<StorageOptions> options)
    {
        this.shards = shards;
        this.options = options;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [RequestSizeLimit(Const.ShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.ShardSizeLimit)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        [FromForm] RequestData request,
        CancellationToken cancellationToken = default)
    {
        var tmpDir = new DirectoryInfo(options.Value.TempDirectory);
        if (!tmpDir.Exists)
        {
            tmpDir.Create();
        }

        var tmpFile = new FileInfo(Path.Combine(options.Value.TempDirectory, $"{Hrib.Create()}.tmp"));
        Hrib? id = null;
        using (var tmpFileStream = new FileStream(tmpFile.FullName, FileMode.Create, FileAccess.Write))
        {
            using var stream = request.File.OpenReadStream();
            await stream.CopyToAsync(tmpFileStream, cancellationToken);
        }

        using (var tmpFileStream = new FileStream(tmpFile.FullName, FileMode.Open, FileAccess.Read))
        {

            id = await shards.Create(
                new ShardCreationDto(request.Kind, request.ArtifactId),
                tmpFileStream,
                cancellationToken);
        }

        tmpFile.Delete();
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
