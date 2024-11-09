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

namespace Kafe.Api.Endpoints.Organization;

[ApiVersion("1")]
[Route("organizations")]
public class OrganizationListEndpoint : EndpointBaseAsync
    .WithRequest<OrganizationListEndpoint.RequestData>
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
    [SwaggerOperation(Tags = new[] { EndpointArea.Organization })]
    public override async Task<ActionResult<ImmutableArray<OrganizationListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var filter = new OrganizationService.OrganizationFilter(
            AccessingAccountId: userProvider.AccountId
        );

        return Ok((await organizationService.List(filter, requestData.Sort, cancellationToken))
            .Select(TransferMaps.ToOrganizationListDto)
            .ToImmutableArray());
    }

    public record RequestData
    {
        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name.iv";
    }
}
