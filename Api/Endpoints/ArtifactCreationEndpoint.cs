using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints;

[ApiVersion("1.0")]
[Route("artifact")]
[Authorize]
public class ArtifactCreationEndpoint : EndpointBaseAsync
    .WithRequest<ArtifactCreationDto>
    .WithActionResult<Hrib>
{
    private readonly IArtifactService artifacts;

    public ArtifactCreationEndpoint(IArtifactService artifacts)
    {
        this.artifacts = artifacts;
    }

    [HttpPost]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ArtifactCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var hrib = await artifacts.Create(request, cancellationToken);
        return Ok(hrib);
    }
}
