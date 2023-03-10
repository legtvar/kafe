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
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using System.Collections.Immutable;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("authors")]
[Authorize]
public class AuthorListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<AuthorListDto>>
{
    private readonly IAuthorService authors;

    public AuthorListEndpoint(IAuthorService authors)
    {
        this.authors = authors;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<ImmutableArray<AuthorListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return Ok(await authors.List(cancellationToken));
    }
}
