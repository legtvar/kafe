using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Services;
using Kafe.Media.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Shard;

[ApiVersion("1")]
[Route("pigeon/{id}")]
[Authorize]
public class PigeonTestEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<Hrib>
{
    private readonly ShardService shardService;
    private readonly ArtifactService artifactService;
    private readonly IAuthorizationService authorizationService;

    public PigeonTestEndpoint(
        ShardService shardService,
        ArtifactService artifactService,
        IAuthorizationService authorizationService)
    {
        this.shardService = shardService;
        this.artifactService = artifactService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Shard })]
    [RequestSizeLimit(Const.ShardSizeLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = Const.ShardSizeLimit)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var shard = await shardService.Load(id, cancellationToken);
        if (shard is null)
        {
            return NotFound();
        }
        
        var auth = await authorizationService.AuthorizeAsync(User, shard.ArtifactId, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
        if (await shardService.GetShardKind(id, cancellationToken) != ShardKind.Blend)
        {
            return BadRequest("The shard is not a Blend file.");
        }
        var shardPath = await shardService.GetShardPath(id, cancellationToken);
        var projectGroupNames = await artifactService.GetArtifactProjectGroupNames(id, cancellationToken);
        if (projectGroupNames.Length != 1)
        {
            throw new InvalidOperationException("A blend shard must belong to exactly one project group.");
        }
        var projectGroupName = projectGroupNames[0]["iv"];

        var service = new PigeonsCoreService();
        var result = await service.RunPigeonsTest(id, shardPath, projectGroupName);

        return Ok();
    }
}
