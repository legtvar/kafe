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
[Authorize]
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
        var entity = await entityService.Load(dto.Id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity is ProjectInfo project)
        {
            var auth = await authorizationService.AuthorizeAsync(User, project.ProjectGroupId, EndpointPolicy.Write);
            if (!auth.Succeeded)
            {
                return Unauthorized();
            }
        }

        var authEntity = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!authEntity.Succeeded)
        {
            return Unauthorized();
        }

        var accountPermissions = dto.AccountPermissions ?? ImmutableArray<EntityPermissionsAccountEditDto>.Empty;
        if (accountPermissions.Any(a => string.IsNullOrEmpty(a.Id?.Value) && string.IsNullOrEmpty(a.EmailAddress)))
        {
            return ValidationProblem(title: "All accounts must be identified by either id or email address.");
        }

        // NB: Npgsql does not allow multiple queries per connection.
        var accounts = new List<(EntityPermissionsAccountEditDto dto, AccountInfo? entity)>(accountPermissions.Length);
        foreach (var accountPerm in accountPermissions)
        {
            var account = accountPerm.Id is not null
                ? await accountService.Load(accountPerm.Id, cancellationToken)
                : await accountService.FindByEmail(accountPerm.EmailAddress!, cancellationToken);
            accounts.Add((dto: accountPerm, entity: account));
        }

        if (accounts.Any(a => a.entity is null))
        {
            var notFoundAccounts = string.Join(", ", accounts.Where(a => a.entity is null)
                .Select(a => $"\"{a.dto.Id ?? a.dto.EmailAddress}\""));
            return NotFound($"Could not find accounts: {notFoundAccounts}.");
        }

        foreach (var account in accounts)
        {
            await entityService.SetPermissions(
                dto.Id,
                TransferMaps.FromPermissionArray(account.dto.Permissions),
                account.entity!.Id,
                cancellationToken);
        }

        if (dto.GlobalPermissions is not null)
        {
            await entityService.SetPermissions(
                entityId: (Hrib)dto.Id,
                permissions: TransferMaps.FromPermissionArray(dto.GlobalPermissions),
                accessingAccountId: null, // sets global permissions
                token: cancellationToken);
        }

        return Ok(dto.Id);
    }
}
