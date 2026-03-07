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
public class RoleListEndpoint(
    RoleService roleService,
    UserProvider userProvider
) : EndpointBaseAsync
    .WithRequest<RoleListEndpoint.RequestData>
    .WithActionResult<ImmutableArray<RoleListDto>>
{
    [HttpGet]
    [SwaggerOperation(Tags = [EndpointArea.Role])]
    public override async Task<ActionResult<ImmutableArray<RoleListDto>>> HandleAsync(
        RequestData requestData,
        CancellationToken ct = default
    )
    {
        var filter = new RoleService.RoleFilter(
            AccessingAccountId: userProvider.AccountId
        );

        return Ok(
            (await roleService.List(filter, requestData.Sort, ct))
            .Select(TransferMaps.ToRoleListDto)
            .ToImmutableArray()
        );
    }

    public record RequestData
    {
        [FromQuery(Name = "sort")]
        public string? Sort { get; set; } = "name.iv";
    }
}
