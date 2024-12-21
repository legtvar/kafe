using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("shard/{id}")]
public class ShardDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ShardDetailBaseDto?>
{
    private readonly ShardService shardService;
    private readonly IAuthorizationService authorizationService;

    public ShardDetailEndpoint(
        ShardService shardService,
        ArtifactService artifactService,
        IAuthorizationService authorizationService)
    {
        this.shardService = shardService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [Tags(EndpointArea.Shard)]
    [ProducesResponseType(typeof(ShardDetailBaseDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<ShardDetailBaseDto?>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var detail = await shardService.Load(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }
        
        var auth = await authorizationService.AuthorizeAsync(User, detail.ArtifactId, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        return Ok(TransferMaps.ToShardDetailDto(detail));
    }
}
