using System;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard/{id}")]
[Obsolete("This endpoint is part of the old artifact abstraction and will soon be replaced.")]
public class ShardDetailEndpoint(
    ShardService shardService,
    IAuthorizationService authorizationService
) : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ShardDetailBaseDto?>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Shard])]
    [ProducesResponseType(typeof(ShardDetailBaseDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ShardDetailBaseDto?>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var shardErr = await shardService.Load(id, cancellationToken);
        if (shardErr.HasError)
        {
            return this.KafeErrResult(shardErr);
        }

        var shard = shardErr.Value;
        var auth = await authorizationService.AuthorizeAsync(User, shard.Id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        return Ok(TransferMaps.ToShardDetailDto(shard));
    }
}
