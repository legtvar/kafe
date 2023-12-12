using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Transfer;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Kafe.Api.Services;
using System.Collections.Immutable;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("authors")]
public class AuthorListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<AuthorListDto>>
{
    private readonly AuthorService authorService;
    private readonly UserProvider userProvider;

    public AuthorListEndpoint(
        AuthorService authorService,
        UserProvider userProvider)
    {
        this.authorService = authorService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<ImmutableArray<AuthorListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = new AuthorService.AuthorFilter(
            AccessingAccountId: userProvider.Account?.Id
        );
        
        return Ok((await authorService.List(filter, cancellationToken))
            .Select(TransferMaps.ToAuthorListDto)
            .ToImmutableArray());
    }
}
