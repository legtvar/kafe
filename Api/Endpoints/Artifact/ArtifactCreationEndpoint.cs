using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Endpoints.Artifact;

[ApiVersion("1")]
[Route("artifact")]
public class ArtifactCreationEndpoint : EndpointBaseAsync
    .WithRequest<ArtifactCreationDto>
    .WithActionResult<Hrib>
{
    private readonly ArtifactService artifacts;
    private readonly IAuthorizationService authorization;

    public ArtifactCreationEndpoint(
        ArtifactService artifacts,
        IAuthorizationService authorization)
    {
        this.artifacts = artifacts;
        this.authorization = authorization;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Artifact })]
    [ProducesResponseType(typeof(Hrib), 200)]
    [ProducesResponseType(400)]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        ArtifactCreationDto request,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorization.AuthorizeAsync(User, request.ContainingProject, EndpointPolicy.Append);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }
        
        var hrib = await artifacts.Create(
            name: request.Name,
            addedOn: request.AddedOn,
            containingProject: request.ContainingProject,
            blueprintSlot: request.BlueprintSlot,
            token: cancellationToken);
        return Ok(hrib);
    }
}
