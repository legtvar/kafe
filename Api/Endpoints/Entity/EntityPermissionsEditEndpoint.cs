using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Entity;

[ApiVersion("1")]
[Route("entity/perms")]
public class EntityPermissionsEditEndpoint : EndpointBaseAsync
    .WithRequest<EntityPermissionsEditDto>
    .WithActionResult<string>
{
    private readonly IAuthorizationService authorizationService;
    private readonly EntityService entityService;
    private readonly UserProvider userProvider;
    private readonly AccountService accountService;

    public EntityPermissionsEditEndpoint(
        IAuthorizationService authorizationService,
        EntityService entityService,
        UserProvider userProvider,
        AccountService accountService)
    {
        this.authorizationService = authorizationService;
        this.entityService = entityService;
        this.userProvider = userProvider;
        this.accountService = accountService;
    }

    [HttpPatch]
    [SwaggerOperation(Tags = new[] { EndpointArea.Entity })]
    public override async Task<ActionResult<string>> HandleAsync(
        EntityPermissionsEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var authEntity = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!authEntity.Succeeded)
        {
            return Unauthorized();
        }

        if (dto.GlobalPermissions is not null)
        {
            await entityService.SetPermissions((Hrib)dto.Id, TransferMaps.FromPermissionArray(dto.GlobalPermissions), userProvider.Account?.Id, cancellationToken);
        }

        // TODO: Apply account changes
        // var authAccount = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        // if (!authEntity.Succeeded)
        // {
        //     return Unauthorized();
        // }

        return Ok(dto.Id);
    }
}
