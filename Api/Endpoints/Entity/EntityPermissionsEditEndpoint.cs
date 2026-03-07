using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Kafe.Api.Endpoints.Entity;

[ApiVersion("1")]
[Route("entity/perms")]
[Authorize]
public class EntityPermissionsEditEndpoint(
    IAuthorizationService authorizationService,
    EntityService entityService,
    UserProvider userProvider,
    AccountService accountService,
    IEmailService emailService,
    IOptions<ApiOptions> apiOptions
) : EndpointBaseAsync
    .WithRequest<EntityPermissionsEditDto>
    .WithActionResult<string>
{
    [HttpPatch]
    [SwaggerOperation(Tags = [EndpointArea.Entity])]
    public override async Task<ActionResult<string>> HandleAsync(
        EntityPermissionsEditDto dto,
        CancellationToken ct = default
    )
    {
        // TODO: Remove this hack once permission masks for project groups are implemented.
        if (dto.Id != Hrib.System)
        {
            var entity = await entityService.Load(dto.Id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity is ProjectInfo project)
            {
                var auth = await authorizationService.AuthorizeAsync(
                    User,
                    project.ProjectGroupId,
                    EndpointPolicy.Write
                );
                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
            }
        }

        var authEntity = await authorizationService.AuthorizeAsync(User, dto.Id, EndpointPolicy.Write);
        if (!authEntity.Succeeded)
        {
            return Unauthorized();
        }

        var accountPermissions = dto.AccountPermissions ?? ImmutableArray<EntityPermissionsAccountEditDto>.Empty;
        if (accountPermissions.Any(a => string.IsNullOrEmpty(a.Id?.ToString())
                && string.IsNullOrEmpty(a.EmailAddress)
            ))
        {
            return ValidationProblem(title: "All accounts must be identified by either id or email address.");
        }

        // NB: Npgsql does not allow multiple queries per connection.
        var accounts =
            new List<(EntityPermissionsAccountEditDto dto, Err<AccountInfo> entity)>(accountPermissions.Length);
        foreach (var accountPerm in accountPermissions)
        {
            var account = accountPerm.Id is not null
                ? await accountService.Load(accountPerm.Id, ct)
                : await accountService.FindByEmail(accountPerm.EmailAddress!, ct);
            accounts.Add((dto: accountPerm, entity: account));
        }

        var unrecoverableErrors = accounts
            .Where(a => a.entity is { HasError: true, Diagnostic.Payload: not NotFoundDiagnostic })
            .Select(a => a.entity.Diagnostic)
            .ToImmutableArray();
        if (unrecoverableErrors.Length > 0)
        {
            return this.KafeErrorResult(Diagnostic.Aggregate(unrecoverableErrors));
        }

        // set permissions for found accounts
        foreach (var account in accounts.Where(a => a.entity is { HasError: false }))
        {
            await entityService.SetPermissions(
                dto.Id,
                TransferMaps.FromPermissionArray(account.dto.Permissions),
                // NB: Must be non-null because they were all found.
                account.entity.Value!.Id,
                ct
            );
        }

        if (dto.GlobalPermissions is not null)
        {
            await entityService.SetPermissions(
                entityId: dto.Id,
                permissions: TransferMaps.FromPermissionArray(dto.GlobalPermissions),
                accessingAccountId: Hrib.Empty, // sets global permissions
                token: ct
            );
        }

        // set invites for nonexistent accounts
        foreach (var account in accounts.Where(a => a.entity is
                { HasError: true, Diagnostic.Payload: NotFoundDiagnostic }
            ))
        {
            if (account.dto.EmailAddress is null)
            {
                continue;
            }

            var invite = await accountService.UpsertInvite(
                invite: InviteInfo.Create(account.dto.EmailAddress.Trim()) with
                {
                    Permissions = ImmutableDictionary.CreateRange<string, InvitePermissionEntry>(
                        [
                            new KeyValuePair<string, InvitePermissionEntry>(
                                dto.Id.ToString(),
                                new InvitePermissionEntry(
                                    EntityId: dto.Id.ToString(),
                                    Permission: TransferMaps.FromPermissionArray(account.dto.Permissions),
                                    InviterAccountId: null
                                )
                            )
                        ]
                    )
                },
                inviterAccountId: userProvider.AccountId,
                ct: ct
            );
            if (invite.HasError)
            {
                return this.KafeErrorResult(invite.Diagnostic);
            }

            // TODO: allow inviting users with a specific culture
            var ticket = await accountService.IssueLoginTicket(
                invite.Value.EmailAddress,
                preferredCulture: null,
                ct: ct
            );
            if (ticket.HasError)
            {
                return this.KafeErrorResult(ticket.Diagnostic);
            }

            var confirmationToken = accountService.EncodeLoginTicketId(ticket.Value.Id);
            var pathString = new PathString(apiOptions.Value.AccountConfirmPath)
                .Add(new PathString("/" + confirmationToken));
            var confirmationUrl = new Uri(new Uri(apiOptions.Value.BaseUrl), pathString);
            // TODO: Custom email template for invites.
            var emailSubject = Const.ConfirmationEmailSubject[ticket.Value.PreferredCulture]!;
            var emailMessage = string.Format(
                Const.ConfirmationEmailMessageTemplate[ticket.Value.PreferredCulture]!,
                confirmationUrl,
                Const.EmailSignOffs[RandomNumberGenerator.GetInt32(0, Const.EmailSignOffs.Length)][
                    ticket.Value.PreferredCulture]
            );
            await emailService.SendEmail(ticket.Value.EmailAddress, emailSubject, emailMessage, null, ct);
        }

        return Ok(dto.Id);
    }
}
