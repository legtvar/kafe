using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using Kafe.Data.Services;
using System.Linq;
using System.Collections.Immutable;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlists")]
public class PlaylistListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<PlaylistListDto>>
{
    private readonly PlaylistService playlistService;
    private readonly IAuthorizationService authorization;

    public PlaylistListEndpoint(
        PlaylistService playlistService,
        IAuthorizationService authorization)
    {
        this.playlistService = playlistService;
        this.authorization = authorization;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    public override async Task<ActionResult<List<PlaylistListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: Filter by permission
        var auth = await authorization.AuthorizeAsync(User, EndpointPolicy.Read);

        var list = await playlistService.List(cancellationToken);
        return Ok(list.Select(TransferMaps.ToPlaylistListDto).ToImmutableArray());
    }
}
