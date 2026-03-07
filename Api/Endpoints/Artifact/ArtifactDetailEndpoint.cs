using System;
using System.Collections.Immutable;
using System.Linq;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core;

namespace Kafe.Api.Endpoints.Artifact;

[ApiVersion("1")]
[Route("artifact/{id}")]
[Obsolete("This endpoint is part of the old artifact abstraction and will soon be replaced.")]
public class ArtifactDetailEndpoint(
    ArtifactService artifacts,
    ShardService shards,
    IAuthorizationService authorization
) : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ArtifactDetailDto>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Artifact])]
    [ProducesResponseType(typeof(Hrib), 200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<ArtifactDetailDto>> HandleAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var auth = await authorization.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var artifactErr = await artifacts.Load(id, ct);
        if (artifactErr.HasError)
        {
            return this.KafeErrResult(artifactErr);
        }

        var containingProjectsErr = await artifacts.GetContainingProjects(id, ct);
        if (containingProjectsErr.HasError)
        {
            return this.KafeErrResult(containingProjectsErr);
        }

        var shardIds = artifactErr.Value.Properties.Values.Select(p => p.Value)
            .OfType<ShardReference>()
            .Select(s => s.ShardId)
            .ToImmutableArray();
        var shardsErr = await shards.LoadMany(shardIds, ct);
        if (shardsErr.HasError)
        {
            return this.KafeErrResult(shardsErr);
        }

        var result = TransferMaps.ToArtifactDetailDto(artifactErr.Value) with
        {
            Shards = [..shardsErr.Value.Select(TransferMaps.ToShardListDto)],
            ContainingProjectIds = [..containingProjectsErr.Value.Select(p => p.Id)]
        };
        return result;
    }
}
