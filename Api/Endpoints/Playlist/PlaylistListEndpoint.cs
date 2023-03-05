using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Playlist;

[ApiVersion("1")]
[Route("playlists")]
[Authorize]
public class PlaylistListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<PlaylistListDto>>
{
    private readonly IQuerySession db;

    public PlaylistListEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Playlist })]
    public override async Task<ActionResult<List<PlaylistListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var playlists = await db.Query<PlaylistInfo>().ToListAsync(cancellationToken);
        return Ok(playlists.Select(TransferMaps.ToPlaylistListDto).ToList());
    }
}
