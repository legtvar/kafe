using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard/{id}")]
[Authorize]
public class ShardDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ShardDetailBaseDto?>
{
    private readonly IShardService shards;

    public ShardDetailEndpoint(IShardService shards)
    {
        this.shards = shards;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [ProducesResponseType(typeof(ShardDetailBaseDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ShardDetailBaseDto?>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var detail = await shards.Load(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return Ok(detail);
    }
}
