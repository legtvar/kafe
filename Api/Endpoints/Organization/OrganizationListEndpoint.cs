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
[Route("organizations")]
public class OrganizationListEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<ImmutableArray<OrganizationListDto>>
{
    private readonly OrganizationService organizationService;
    private readonly UserProvider userProvider;

    public OrganizationListEndpoint(
        OrganizationService organizationService,
        UserProvider userProvider)
    {
        this.organizationService = organizationService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Author })]
    public override async Task<ActionResult<ImmutableArray<OrganizationListDto>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = new OrganizationService.OrganizationFilter(
            AccessingAccountId: userProvider.AccountId
        );

        return Ok((await organizationService.List(filter, cancellationToken))
            .Select(TransferMaps.ToOrganizationListDto)
            .ToImmutableArray());
    }
}
