using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Artifact;

[ApiVersion("1")]
[Route("artifact/{id}")]
[Authorize(Policy = EndpointPolicy.AdministratorOnly)]
public class ArtifactDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<ArtifactDetailDto>
{
    private readonly IArtifactService artifacts;

    public ArtifactDetailEndpoint(IArtifactService artifacts)
    {
        this.artifacts = artifacts;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Artifact })]
    [ProducesResponseType(typeof(Hrib), 200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<ArtifactDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var dto = await artifacts.Load(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
