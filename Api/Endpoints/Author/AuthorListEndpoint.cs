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
using Kafe.Data.Services;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("authors")]
public class AuthorListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<AuthorListDto>>
{
    private readonly AuthorService authorService;
    private readonly IAuthorizationService authorizationService;

    public AuthorListEndpoint(
        AuthorService authorService,
        IAuthorizationService authorizationService)
    {
        this.authorService = authorService;
        this.authorizationService = authorizationService;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<ImmutableArray<AuthorListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: Filter by permission
        var auth = await authorizationService.AuthorizeAsync(User, EndpointPolicy.Inspect);
        
        return Ok((await authorService.List(cancellationToken))
            .Select(TransferMaps.ToAuthorListDto)
            .ToImmutableArray());
    }
}
