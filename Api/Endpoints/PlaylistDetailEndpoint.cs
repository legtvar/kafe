using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Api.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints;

[ApiVersion("1")]
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
    [SwaggerOperation(Tags = new[] { SwaggerTags.Playlist })]
    [ProducesResponseType(typeof(PlaylistDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<PlaylistDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var data = await db.LoadAsync<PlaylistInfo>(id, token: cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToPlaylistDetailDto(data));
    }
}
