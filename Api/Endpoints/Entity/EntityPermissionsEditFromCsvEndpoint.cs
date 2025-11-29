using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Asp.Versioning;
using Kafe.Api.Options;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Kafe.Data; // Add this to import Permission type
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Routing;

namespace Kafe.Api.Endpoints.Entity;

[ApiVersion("1")]
[Route("entity/perms-csv")]
[Authorize]
public class EntityPermissionsEditFromCsvEndpoint(
    IAuthorizationService authorizationService,
    EntityService entityService,
    UserProvider userProvider,
    AccountService accountService,
    IEmailService emailService,
    IOptions<ApiOptions> apiOptions,
    LinkGenerator linkGenerator
) : EndpointBaseAsync
    .WithRequest<EntityPermissionsEditFromCsvDto>
    .WithActionResult<string>
{
    [HttpPost]
    [SwaggerOperation(Tags = [EndpointArea.Entity])]
    [Consumes("multipart/form-data")]
    public override async Task<ActionResult<string>> HandleAsync(
        [FromForm] EntityPermissionsEditFromCsvDto dto,
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

        var ucoList = GetUcosFromCsv(dto.CsvFile);
        if (ucoList is null)
        {
            return this.KafeErrorResult(
                Error.InvalidValue("CSV file is not in the correct format.")
            );
        }

        // NB: Npgsql does not allow multiple queries per connection.
        var accounts = new List<(EntityPermissionsAccountEditDto dto, AccountInfo? entity)>(ucoList.Count);
        var notFoundAccounts = new List<string>();
        foreach (var uco in ucoList)
        {
            var account = await accountService.FindByUco(uco, ct);
            if (account is null)
            {
                if (dto.Permissions != null && dto.Permissions.Count > 0)
                {
                    var possibleEmailAddress = $"{uco}@mail.muni.cz";
                    notFoundAccounts.Add(possibleEmailAddress);
                }
            }
            else
            {
                EntityPermissionsAccountEditDto accountPermissions = new(
                    Id: account.Id,
                    EmailAddress: account.EmailAddress,
                    Permissions: dto.Permissions != null ? dto.Permissions.ToImmutableArray() : ImmutableArray<Permission>.Empty
                );
                accounts.Add((dto: accountPermissions, entity: account));
            }
        }
        
        foreach (var emailAddress in notFoundAccounts)
        {
            if (string.IsNullOrWhiteSpace(emailAddress)
                || !AccountService.IsValidEmailAddress(emailAddress))
            {
                return this.KafeErrorResult(
                    Error.InvalidValue($"String '{emailAddress}' is not a valid email address.")
                );
            }
        }

        foreach (var account in accounts)
        {
            await entityService.SetPermissions(
                dto.Id,
                TransferMaps.FromPermissionArray(account.dto.Permissions),
                account.entity!.Id,
                ct
            );
        }

        foreach (var emailAddress in notFoundAccounts)
        {
            var invite = await accountService.UpsertInvite(
                invite: InviteInfo.Create(emailAddress.Trim()) with
                {
                    Permissions = ImmutableDictionary.CreateRange<string, InvitePermissionEntry>([
                            new(
                                dto.Id.ToString(),
                                new InvitePermissionEntry(
                                    EntityId: dto.Id.ToString(),
                                    Permission: TransferMaps.FromPermissionArray(dto.Permissions?.ToImmutableArray()),
                                    InviterAccountId: null
                                )
                            )
                        ]
                    )
                },
                inviterAccountId: userProvider.AccountId,
                ct: ct
            );
            if (invite.HasErrors)
            {
                return this.KafeErrorResult(invite.Errors);
            }

            

            var externalLoginUrl = linkGenerator.GetUriByAction(
                httpContext: HttpContext,
                action: nameof(Account.ExternalAccountLoginEndpoint.Handle),
                controller: nameof(Account.ExternalAccountLoginEndpoint),
                values: new
                {
                    redirect = $"{apiOptions.Value.BaseUrl}/auth",
                    version = "1"
                }
            );
            var inviter = await accountService.Load(userProvider.AccountId, ct);
            var invitation = inviter?.Name != null
            ? string.Format(Const.InvitationTemplate[Const.InvariantCulture], inviter.Name) 
            : Const.InvitationGenericTemplate[Const.InvariantCulture];
            var emailSubject = Const.InvitationEmailSubject[Const.InvariantCulture]!;
            var emailMessage = string.Format(
                Const.InvitationEmailMessageTemplate[Const.InvariantCulture]!,
                invitation,
                externalLoginUrl,
                Const.EmailSignOffs[RandomNumberGenerator.GetInt32(0, Const.EmailSignOffs.Length)][
                    Const.InvariantCulture]
            );
            await emailService.SendEmail(emailAddress, emailSubject, emailMessage, null, ct);
        }

        return Ok(dto.Id);
    }
    private static List<string>? GetUcosFromCsv(IFormFile csvFile)
    {
        var result = new List<string>();

        using var stream = csvFile.OpenReadStream();
        using var reader = new StreamReader(stream);
        string? line;
        bool isFirstLine = true;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line
                .Split(';')
                .Select(p => p.Trim().Trim('"'))
                .ToArray();
            if (parts.Length != 6)
                return null;

            if (isFirstLine)
            {
                isFirstLine = false;
                if (parts[1].Contains("Učo", StringComparison.OrdinalIgnoreCase) ||
                    parts[1].Contains("Personal ident. number (učo)", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {
                    return null;        
                }
            }

            result.Add(parts[1]);
        }

        return result;
    }
}


