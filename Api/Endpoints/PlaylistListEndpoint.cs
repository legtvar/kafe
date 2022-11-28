using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("playlists")]
[Authorize]
public class PlaylistListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<Playlist>>
{
    private readonly IQuerySession db;

    public PlaylistListEndpoint(IQuerySession db)
    {
        this.db = db;
    }
    
    [HttpGet]
    public override async Task<ActionResult<List<Playlist>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await db.Query<Playlist>().ToListAsync());
    }
}
