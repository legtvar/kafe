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

namespace Kafe.Api.Endpoints.Author;

[ApiVersion("1")]
[Route("authors")]
public class AuthorListEndpoint : EndpointBaseAsync
    .WithRequest<AuthorListEndpoint.RequestData>
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
    [SwaggerOperation(Tags = [EndpointArea.Author])]
    public override async Task<ActionResult<ImmutableArray<AuthorListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var filter = new AuthorService.AuthorFilter(
            AccessingAccountId: userProvider.AccountId
        );

        return Ok((await authorService.List(filter, requestData.Sort, cancellationToken))
            .Select(TransferMaps.ToAuthorListDto)
            .ToImmutableArray());
    }

    public record RequestData
    {
        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name";
    }
}
