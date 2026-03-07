using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist")]
[Authorize]
public class PlaylistEditEndpoint(
    PlaylistService playlistService,
    IAuthorizationService authorizationService
)
    : EndpointBaseAsync
        .WithRequest<PlaylistEditDto>
        .WithActionResult<Hrib>
{
    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.Playlist])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        PlaylistEditDto dto,
        CancellationToken ct = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var oldErr = await playlistService.Load(dto.Id, ct);
        if (oldErr.HasError)
        {
            return this.KafeErrResult(oldErr);
        }

        var old = oldErr.Value;
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

        var result = await playlistService.Edit(@new, ct);
        if (result.HasError)
        {
            return this.KafeErrResult(result);
        }

        return Ok(dto.Id);
    }
}
