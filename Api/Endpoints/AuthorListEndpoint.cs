using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Endpoints;

[ApiVersion("1.0")]
[Route("authors")]
[Authorize]
public class AuthorListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<Author>>
{
    private readonly IQuerySession db;

    public AuthorListEndpoint(IQuerySession db)
    {
        this.db = db;
    }
    
    [HttpGet]
    public override async Task<ActionResult<List<Author>>>HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await db.Query<Author>().ToListAsync());
    }
}
