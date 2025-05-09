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

namespace Kafe.Api.Endpoints.Role;

[ApiVersion("1")]
[Route("roles")]
public class RoleListEndpoint : EndpointBaseAsync
    .WithRequest<RoleListEndpoint.RequestData>
    .WithActionResult<ImmutableArray<RoleListDto>>
{
    private readonly RoleService roleService;
    private readonly UserProvider userProvider;

    public RoleListEndpoint(
        RoleService roleService,
        UserProvider userProvider)
    {
        this.roleService = roleService;
        this.userProvider = userProvider;
    }

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Role })]
    public override async Task<ActionResult<ImmutableArray<RoleListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken cancellationToken = default)
    {
        var filter = new RoleService.RoleFilter(
            AccessingAccountId: userProvider.AccountId
        );

        return Ok((await roleService.List(filter, requestData.Sort, cancellationToken))
            .Select(TransferMaps.ToRoleListDto)
            .ToImmutableArray());
    }
    
    public record RequestData
    {
        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name.iv";
    }
}
