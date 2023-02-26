using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Data.Aggregates;
using Kafe.Api.Transfer;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api.Endpoints;

[ApiVersion("1.0")]
[Route("authors")]
[Authorize]
public class AuthorListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<AuthorListDto>>
{
    private readonly IQuerySession db;

    public AuthorListEndpoint(IQuerySession db)
    {
        this.db = db;
    }

    [HttpGet]
    public override async Task<ActionResult<List<AuthorListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var authors = await db.Query<AuthorInfo>().ToListAsync(cancellationToken);
        return Ok(authors.Select(TransferMaps.ToAuthorListDto).ToList());
    }
}
