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

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlists")]
[Authorize]
public class PlaylistListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<PlaylistListDto>>
{
    private readonly IPlaylistService playlists;

    public PlaylistListEndpoint(IPlaylistService playlists)
    {
        this.playlists = playlists;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    public override async Task<ActionResult<List<PlaylistListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await playlists.List(cancellationToken);
        return Ok(list);
    }
}
