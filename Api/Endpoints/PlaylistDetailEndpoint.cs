using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("playlist/{id}")]
[Authorize]
public class PlaylistDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<PlaylistDetailDto>
{
    private readonly IQuerySession db;

    public PlaylistDetailEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    public override async Task<ActionResult<PlaylistDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var data = await db.Events.AggregateStreamAsync<PlaylistInfo>(id, token: cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToPlaylistDetailDto(data));
    }
}
