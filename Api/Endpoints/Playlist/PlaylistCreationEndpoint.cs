using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Data.Services;
using System.Collections.Immutable;
using Kafe.Data.Aggregates;
using Kafe.Data;
using Kafe.Api.Services;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist")]
[Authorize]
public class PlaylistCreationEndpoint(
    PlaylistService playlistService,
    UserProvider userProvider,
    IAuthorizationService authorizationService
)
    : EndpointBaseAsync
        .WithRequest<PlaylistCreationDto>
        .WithActionResult<Hrib>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Playlist])]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        PlaylistCreationDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var auth = await authorizationService.AuthorizeAsync(User, dto.OrganizationId, EndpointPolicy.Write);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var playlist = await playlistService.Create(
            @new: PlaylistInfo.Create(dto.OrganizationId, dto.Name) with
            {
                EntryIds = dto.EntryIds ?? ImmutableArray<string>.Empty,
                Description = dto.Description,
                GlobalPermissions = dto.GlobalPermissions is not null
                    ? TransferMaps.FromPermissionArray(dto.GlobalPermissions)
                    : Permission.None
            },
            ownerId: userProvider.AccountId,
            token: cancellationToken
        );

        if (playlist.HasError)
        {
            return this.KafeErrResult(playlist);
        }

        return Ok((Hrib)playlist.Value.Id);
    }
}
