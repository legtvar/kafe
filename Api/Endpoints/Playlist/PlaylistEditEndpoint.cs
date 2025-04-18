using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;
using System.Linq;
using System.Collections.Immutable;
using Kafe.Data.Aggregates;
using Kafe.Data;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.Mime;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist")]
[Authorize]
public class PlaylistEditEndpoint : EndpointBaseAsync
    .WithRequest<PlaylistEditDto>
    .WithActionResult<Hrib>
{
    private readonly PlaylistService playlistService;
    private readonly ArtifactService artifactService;
    private readonly IAuthorizationService authorizationService;

    public PlaylistEditEndpoint(
        PlaylistService playlistService,
        ArtifactService artifactService,
        IAuthorizationService authorizationService)
    {
        this.playlistService = playlistService;
        this.artifactService = artifactService;
        this.authorizationService = authorizationService;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.Playlist])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        PlaylistEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var old = await playlistService.Load(dto.Id, cancellationToken);
        if (old is null)
        {
            return NotFound();
        }

        var @new = old with
        {
            Name = dto.Name ?? old.Name,
            Description = dto.Description ?? old.Description,
            GlobalPermissions = dto.GlobalPermissions is not null
                ? TransferMaps.FromPermissionArray(dto.GlobalPermissions)
                : old.GlobalPermissions,
            EntryIds = dto.EntryIds ?? old.EntryIds,
            OrganizationId = dto.OrganizationId?.ToString() ?? old.OrganizationId
        };

        if (@new.OrganizationId != old.OrganizationId)
        {
            var oldOrgAuth = await authorizationService.AuthorizeAsync(User, old.OrganizationId, EndpointPolicy.Write);
            if (!oldOrgAuth.Succeeded)
            {
                return Unauthorized($"You don't have write permissions to organization '{old.OrganizationId}'.");
            }

            var newOrgAuth = await authorizationService.AuthorizeAsync(User, @new.OrganizationId, EndpointPolicy.Write);
            if (!newOrgAuth.Succeeded)
            {
                return Unauthorized($"You don't have write permissions to organization '{@new.OrganizationId}'.");
            }
        }

        var result = await playlistService.Edit(@new, cancellationToken);
        if (result.Diagnostic is not null)
        {
            return this.KafeErrResult(result);
        }

        return Ok(dto.Id);
    }
}
