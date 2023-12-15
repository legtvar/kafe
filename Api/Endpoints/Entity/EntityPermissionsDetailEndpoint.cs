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
[Route("entity/perms/{id}")]
public class EntityPermissionsDetailEndpoint : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<EntityPermissionsDetailDto>
{
    private readonly IAuthorizationService authorizationService;
    private readonly EntityService entityService;
    private readonly UserProvider userProvider;
    private readonly AccountService accountService;

    public EntityPermissionsDetailEndpoint(
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

    [HttpGet]
    [SwaggerOperation(Tags = new[] { EndpointArea.Entity })]
    public override async Task<ActionResult<EntityPermissionsDetailDto>> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var auth = await authorizationService.AuthorizeAsync(User, id, EndpointPolicy.Read);
        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var entityId = (Hrib)id;
        IEntity? entity = null;

        if (entityId != Hrib.System)
        {
            entity = await entityService.Load(entityId, cancellationToken);
            if (entity is null)
            {
                return NotFound();
            }
        }

        var userPermissions = await entityService.GetPermission(entityId, userProvider.Account?.Id, cancellationToken);

        var relevantAccounts = await accountService.List(new()
        {
            Permissions = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, Permission>(id, Permission.None)
            })
        });

        return Ok(TransferMaps.ToEntityPermissionsDetailDto(
            id: entityId,
            entityType: entityId == Hrib.System ? null : entity?.GetType().Name,
            globalPermissions: entity is IVisibleEntity visible ? visible.GlobalPermissions : null,
            userPermissions: userPermissions,
            accounts: relevantAccounts.OrderBy(a => a.EmailAddress)));
    }
}
