using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Artifact;

[ApiVersion("1")]
[Route("artifact/{id}")]
public class ArtifactDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ArtifactDetailDto>
{
    private readonly ArtifactService artifacts;
    private readonly IAuthorizationService authorization;

    public ArtifactDetailEndpoint(
        ArtifactService artifacts,
        IAuthorizationService authorization)
    {
        this.artifacts = artifacts;
        this.authorization = authorization;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Artifact })]
    [ProducesResponseType(typeof(Hrib), 200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<ArtifactDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorization.AuthorizeAsync(User, EndpointPolicy.ReadInspect);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var artifact = await artifacts.Load(id, cancellationToken);
        return artifact is null ? NotFound() : Ok(TransferMaps.ToArtifactDetailDto(artifact));
    }
}
