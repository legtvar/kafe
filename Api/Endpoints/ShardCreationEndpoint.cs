using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Swagger;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
[Route("shard")]
[Authorize]
public class ShardCreationEndpoint : EndpointBaseAsync
    .WithRequest<ShardCreationEndpoint.RequestData>
    .WithActionResult<Hrib>
{
    private readonly IShardService shards;

    public ShardCreationEndpoint(IShardService shards)
    {
        this.shards = shards;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Shard })]
    [RequestSizeLimit(Const.ShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.ShardSizeLimit)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        [FromRoute]RequestData request,
        CancellationToken cancellationToken = default)
    {
        using var stream = request.File.OpenReadStream();
        var id = await shards.Create(
            request.Dto,
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
        [FromBody] ShardCreationDto Dto,
        [FromBody] IFormFile File);
}
