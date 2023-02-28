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
[Route("author/{id}")]
[Authorize]
public class AuthorDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<AuthorDetailDto>
{
    private readonly IQuerySession db;

    public AuthorDetailEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { SwaggerTags.Author })]
    [ProducesResponseType(typeof(AuthorDetailDto), 200)]
    [ProducesResponseType(404)]
    public override async Task<ActionResult<AuthorDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var data = await db.LoadAsync<AuthorInfo>(id, token: cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        return Ok(TransferMaps.ToAuthorDetailDto(data));
    }
}
