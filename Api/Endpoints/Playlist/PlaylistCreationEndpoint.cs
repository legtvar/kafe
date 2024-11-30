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

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlist")]
[Authorize(EndpointPolicy.Write)]
public class PlaylistCreationEndpoint : EndpointBaseAsync
    .WithRequest<PlaylistCreationDto>
    .WithActionResult<Hrib>
{
    private readonly PlaylistService playlistService;
    private readonly ArtifactService artifactService;
    private readonly IAuthorizationService authorizationService;

    public PlaylistCreationEndpoint(
        PlaylistService playlistService,
        ArtifactService artifactService,
        IAuthorizationService authorizationService)
    {
        this.playlistService = playlistService;
        this.artifactService = artifactService;
        this.authorizationService = authorizationService;
    }

    [HttpPost]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    public override async Task<ActionResult<Hrib>> HandleAsync(
        PlaylistCreationDto dto,
        CancellationToken cancellationToken = default)
    {
        var playlist = await playlistService.Create(PlaylistInfo.Create(dto.OrganizationId, dto.Name) with
        {
            EntryIds = dto.EntryIds ?? ImmutableArray<string>.Empty,
            Description = dto.Description,
            GlobalPermissions = dto.GlobalPermissions is not null
                    ? TransferMaps.FromPermissionArray(dto.GlobalPermissions)
                    : Permission.None
        }, cancellationToken);

        if (playlist.HasErrors)
        {
            return this.KafeErrResult(playlist);
        }

        return Ok((Hrib)playlist.Value.Id);
    }
}
