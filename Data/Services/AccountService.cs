using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Documents;
using Marten.Events;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Kafe.Data.Services;

public class AccountService(
    IDocumentSession db,
    EntityMetadataProvider entityMetadataProvider,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<AccountService> logger
)
{
    public static readonly TimeSpan ConfirmationTokenExpiration = TimeSpan.FromHours(24);
    private readonly IDataProtector dataProtector = dataProtectionProvider.CreateProtector(nameof(AccountService));
    public const string NameClaim = "name";
    public const string PreferredUsernameClaim = "preferred_username";

    public async Task<AccountInfo?> Load(
        Hrib id,
        CancellationToken token = default
    )
    {
        return (await db.LoadAsync<AccountInfo>(id, token: token)).GetValueOrDefault();
    }

    public async Task<AccountInfo?> FindByEmail(string emailAddress, CancellationToken token = default)
    {
        return await db.Query<AccountInfo>()
            .Where(a => a.EmailAddress == emailAddress)
            .SingleOrDefaultAsync(token: token);
    }

    public async Task<AccountInfo?> FindByUco(string uco, CancellationToken token = default)
    {
        return await db.Query<AccountInfo>()
            .Where(a => a.Uco == uco)
            .SingleOrDefaultAsync(token: token);
    }

    public async Task<LoginTicketInfo?> LoadTicket(Guid id, CancellationToken ct = default)
    {
        return await db.LoadAsync<LoginTicketInfo>(id, ct);
    }

    public async Task<InviteInfo?> LoadInvite(Hrib id, CancellationToken ct = default)
    {
        return (await db.KafeLoadAsync<InviteInfo>(id, ct)).GetValueOrDefault();
    }

    public async Task<InviteInfo?> FindInviteByEmail(string emailAddress, CancellationToken token = default)
    {
        return await db.Query<InviteInfo>()
            .Where(i => !i.Deleted && i.EmailAddress == emailAddress)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(token);
    }

    public async Task<Err<AccountInfo>> Create(AccountInfo @new, CancellationToken token = default)
    {
        if (!Hrib.TryParse(@new.Id, out var id, out _))
        {
            return diagnosticFactory.FromPayload(new BadHribDiagnostic(@new.Id));
        }

        if (id == Hrib.Empty)
        {
            id = Hrib.Create();
        }

        var created = new AccountCreated(
            AccountId: id.ToString(),
            CreationMethod: @new.CreationMethod is not CreationMethod.Unknown
                ? @new.CreationMethod
                : CreationMethod.Api,
            EmailAddress: @new.EmailAddress,
            PreferredCulture: @new.PreferredCulture
        );
        var selfPermissionSet = new AccountPermissionSet(id.ToString(), id.ToString(), Permission.All);
        db.Events.KafeStartStream<AccountInfo>(id, created, selfPermissionSet);

        switch (@new.Kind)
        {
            case AccountKind.External:
                if (string.IsNullOrEmpty(@new.IdentityProvider))
                {
                    return diagnosticFactory.FromPayload(
                        payload: new ParameterDiagnostic(
                            nameof(@new.IdentityProvider),
                            diagnosticFactory.FromPayload(new RequiredDiagnostic())),
                        severityOverride: DiagnosticSeverity.Error
                    );
                }

                var associated = new ExternalAccountAssociated(
                    AccountId: id.ToString(),
                    IdentityProvider: @new.IdentityProvider,
                    Name: @new.Name,
                    Uco: @new.Uco
                );
                db.Events.KafeAppend(id, associated);
                break;
        }

        if (!string.IsNullOrEmpty(@new.Uco)
            || !string.IsNullOrEmpty(@new.Name)
            || !string.IsNullOrEmpty(@new.Phone))
        {
            var infoChanged = new AccountInfoChanged(
                AccountId: id.ToString(),
                PreferredCulture: null,
                Name: @new.Name,
                Uco: @new.Uco,
                Phone: @new.Phone
            );
            db.Events.KafeAppend(id, infoChanged);
        }

        foreach (var permission in (@new.Permissions ?? ImmutableDictionary<string, Permission>.Empty))
        {
            db.Events.KafeAppend(
                @new.Id,
                new AccountPermissionSet(
                    AccountId: @new.Id,
                    EntityId: permission.Key,
                    Permission: permission.Value
                )
            );
        }

        foreach (var role in @new.RoleIds)
        {
            db.Events.KafeAppend(
                @new.Id,
                new AccountRoleSet(
                    AccountId: @new.Id,
                    RoleId: role
                )
            );
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<AccountInfo>(id, token: token);
    }

    public async Task<Err<AccountInfo>> Edit(AccountInfo modified, CancellationToken token = default)
    {
        var old = await Load(modified.Id, token);
        if (old is null)
        {
            return diagnosticFactory.NotFound<AccountInfo>(modified.Id);
        }

        var infoChanged = new AccountInfoChanged(
            AccountId: modified.Id,
            PreferredCulture: modified.PreferredCulture != old.PreferredCulture
                ? modified.PreferredCulture
                : null,
            Name: modified.Name != old.Name
                ? modified.Name
                : null,
            Uco: modified.Uco != old.Uco
                ? modified.Uco
                : null,
            Phone: modified.Phone != old.Phone
                ? modified.Phone
                : null
        );

        var hasChanged = false;
        if (infoChanged.PreferredCulture is not null
            || infoChanged.Name is not null
            || infoChanged.Uco is not null
            || infoChanged.Phone is not null)
        {
            hasChanged = true;
            db.Events.KafeAppend(modified.Id, infoChanged);
        }

        var changedPermissions = modified.Permissions.Except(@old.Permissions);
        foreach (var changedPermission in changedPermissions)
        {
            hasChanged = true;
            db.Events.KafeAppend(
                old.Id,
                new AccountPermissionSet(
                    AccountId: old.Id,
                    EntityId: changedPermission.Key,
                    Permission: changedPermission.Value
                )
            );
        }

        var removedPermissions = old.Permissions.Keys.Except(modified.Permissions.Keys);
        foreach (var removedPermission in removedPermissions)
        {
            hasChanged = true;
            db.Events.KafeAppend(
                old.Id,
                new AccountPermissionSet(
                    AccountId: old.Id,
                    EntityId: removedPermission,
                    Permission: Permission.None
                )
            );
        }

        var addedRoles = modified.RoleIds.Except(modified.RoleIds);
        foreach (var newRole in addedRoles)
        {
            hasChanged = true;
            db.Events.KafeAppend(
                old.Id,
                new AccountRoleSet(
                    AccountId: old.Id,
                    RoleId: newRole
                )
            );
        }

        var removedRoles = old.RoleIds.Except(modified.RoleIds);
        foreach (var removedRole in removedRoles)
        {
            hasChanged = true;
            db.Events.KafeAppend(
                old.Id,
                new AccountRoleUnset(
                    AccountId: old.Id,
                    RoleId: removedRole
                )
            );
        }

        if (!hasChanged)
        {
            return diagnosticFactory.Unmodified<AccountInfo>(old.Id);
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<AccountInfo>(old.Id, token: token);
    }

    public record AccountFilter(
        string? Uco = null,
        ImmutableDictionary<string, Permission>? Permissions = null
    );

    public async Task<ImmutableArray<AccountInfo>> List(
        AccountFilter? filter = null,
        string? sort = null,
        CancellationToken token = default
    )
    {
        filter ??= new AccountFilter();

        var query = db.Query<AccountInfo>();
        if (filter.Uco is not null)
        {
            query = (IMartenQueryable<AccountInfo>)query.Where(a => a.Uco == filter.Uco);
        }

        if (filter.Permissions is not null)
        {
            var permValues = string.Join(",", filter.Permissions.Select(p => $"('{p.Key}',{(int)p.Value})"));
            query = (IMartenQueryable<AccountInfo>)query.Where(a => a.MatchesSql(
                    $"""
                     TRUE = ALL(
                         SELECT (data -> 'Permissions' -> entity_id)::int & expected = expected
                         FROM (VALUES {permValues}) as expected_perms (entity_id, expected)
                     )
                     """
                )
            );
        }

        if (!string.IsNullOrEmpty(sort))
        {
            query = (IMartenQueryable<AccountInfo>)query.OrderBySortString(entityMetadataProvider, sort);
        }

        return [.. await query.ToListAsync(token: token)];
    }

    public record InviteFilter(
        ImmutableDictionary<string, Permission>? Permissions = null,
        bool? Deleted = false
    );

    public async Task<ImmutableArray<InviteInfo>> ListInvites(
        InviteFilter? filter = null,
        CancellationToken ct = default
    )
    {
        filter ??= new InviteFilter();

        var query = db.Query<InviteInfo>();

        if (filter.Permissions is not null)
        {
            var permValues = string.Join(",", filter.Permissions.Select(p => $"('{p.Key}',{(int)p.Value})"));
            query = (IMartenQueryable<InviteInfo>)query.Where(a => a.MatchesSql(
                    $"""
                     TRUE = ALL(
                         SELECT (data -> 'Permissions' -> entity_id -> 'Permission')::int & expected = expected
                         FROM (VALUES {permValues}) as expected_perms (entity_id, expected)
                     )
                     """
                )
            );
        }

        if (filter.Deleted is not null)
        {
            query = (IMartenQueryable<InviteInfo>)query.Where(i => i.Deleted == filter.Deleted.Value);
        }

        return [.. await query.ToListAsync(ct)];
    }

    /// <summary>
    /// Gives the account the specified capabilities.
    /// </summary>
    public async Task<Err<bool>> AddPermissions(
        Hrib accountId,
        IEnumerable<(Hrib entityId, Permission permission)> permissions,
        CancellationToken token = default
    )
    {
        // TODO: Find a cheaper way of knowing that an account exists.
        var account = await Load(accountId, token);
        if (account is null)
        {
            return diagnosticFactory.NotFound<AccountInfo>(accountId);
        }

        foreach (var permissionPair in permissions)
        {
            if (!account.Permissions.TryGetValue(permissionPair.entityId.ToString(), out var existingPermission)
                || existingPermission != permissionPair.permission)
            {
                db.Events.KafeAppend(
                    accountId,
                    new AccountPermissionSet(
                        AccountId: accountId.ToString(),
                        EntityId: permissionPair.entityId.ToString(),
                        Permission: permissionPair.permission
                    )
                );
            }
        }

        await db.SaveChangesAsync(token);
        return true;
    }

    public Task<Err<bool>> AddPermissions(
        Hrib accountId,
        CancellationToken token = default,
        params (Hrib entityId, Permission permission)[] permissions
    )
    {
        return AddPermissions(accountId, permissions, token);
    }

    public async Task<Err<bool>> AddRoles(
        Hrib accountId,
        IEnumerable<Hrib> roleIds,
        CancellationToken token = default
    )
    {
        var account = await Load(accountId, token);
        if (account is null)
        {
            return diagnosticFactory.NotFound<AccountInfo>(accountId);
        }

        foreach (var roleId in roleIds)
        {
            if (!account.RoleIds.Contains(roleId.ToString()))
            {
                db.Events.KafeAppend(
                    accountId,
                    new AccountRoleSet(
                        AccountId: accountId.ToString(),
                        RoleId: roleId.ToString()
                    )
                );
            }
        }

        await db.SaveChangesAsync(token);
        return true;
    }

    public Task<Err<bool>> AddRoles(
        Hrib accountId,
        CancellationToken token = default,
        params Hrib[] roleIds
    )
    {
        return AddRoles(accountId, token, roleIds);
    }

    public async Task<Err<AccountInfo>> AssociateExternalAccount(
        ClaimsPrincipal principal,
        CancellationToken token = default
    )
    {
        var emailClaim = principal.FindFirst(ClaimTypes.Email);
        if (emailClaim is null || string.IsNullOrEmpty(emailClaim.Value))
        {
            return diagnosticFactory.ForParameter(ClaimTypes.Email, new RequiredDiagnostic());
        }

        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst(NameClaim)?.Value;
        var uco = principal.FindFirst(PreferredUsernameClaim)?.Value;
        var identityProvider = emailClaim.Issuer;

        var existing = await FindByEmail(emailClaim.Value, token);
        if (existing is not null
            && existing.Kind == AccountKind.External
            && existing.IdentityProvider == identityProvider
            && existing.Name == name
            && existing.Uco == uco
        )
        {
            return existing;
        }

        var id = existing?.Id ?? Hrib.Create().ToString();
        var associated = new ExternalAccountAssociated(
            AccountId: id,
            IdentityProvider: identityProvider,
            Name: name,
            Uco: uco
        );

        if (existing is null)
        {
            var created = new AccountCreated(
                AccountId: id,
                CreationMethod: CreationMethod.Api,
                EmailAddress: emailClaim.Value,
                PreferredCulture: Const.InvariantCultureCode
            );
            db.Events.KafeStartStream<AccountInfo>(id, created, associated);
        }
        else
        {
            db.Events.KafeAppend(id, associated);
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<AccountInfo>(id, token: token);
    }

    public async Task<Err<LoginTicketInfo>> IssueLoginTicket(
        string emailAddress,
        string? preferredCulture,
        CancellationToken ct = default
    )
    {
        emailAddress = emailAddress?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(emailAddress) || !IsValidEmailAddress(emailAddress))
        {
            return Error.InvalidValue("The email address is invalid.", nameof(emailAddress));
        }

        var existingAccount = await FindByEmail(emailAddress, ct);
        var existingInvite = await FindInviteByEmail(emailAddress, ct);

        preferredCulture ??= existingAccount?.PreferredCulture
            ?? existingInvite?.PreferredCulture
            ?? Const.InvariantCultureCode;

        var ticket = new LoginTicketInfo(
            Id: Guid.NewGuid(),
            EmailAddress: emailAddress,
            PreferredCulture: preferredCulture,
            AccountId: existingAccount?.Id,
            InviteId: existingInvite?.Id,
            CreatedAt: DateTimeOffset.UtcNow
        );

        db.Insert(ticket);
        await db.SaveChangesAsync(ct);
        return ticket;
    }

    public async Task<Err<AccountInfo>> PunchTicket(
        Guid loginTicketId,
        bool shouldWaitForDaemon = true,
        CancellationToken ct = default
    )
    {
        var ticket = await db.LoadAsync<LoginTicketInfo>(loginTicketId, ct);
        if (ticket is null)
        {
            return Error.NotFound("The login ticket does not exist.");
        }

        if (ticket.Deleted)
        {
            return Error.InvalidValue("The login ticket is no longer valid.");
        }

        ticket = ticket with { Deleted = true };
        db.Delete(ticket);
        await db.SaveChangesAsync(ct);

        if (ticket.CreatedAt + ConfirmationTokenExpiration < DateTimeOffset.UtcNow)
        {
            return Error.InvalidValue("The login ticket has expired.");
        }

        AccountInfo? account = null;

        if (ticket.AccountId is not null)
        {
            account = await Load(ticket.AccountId, ct);
            if (account is null)
            {
                return Error.NotFound("The account referred to by the login ticket does not exist.");
            }
        }
        else
        {
            var errAccount = await Create(
                AccountInfo.Create(ticket.EmailAddress, ticket.PreferredCulture) with
                {
                    Kind = AccountKind.Temporary
                },
                ct
            );

            if (errAccount.HasErrors)
            {
                return errAccount;
            }

            account = errAccount.Value;
        }

        var accountId = Hrib.Parse(account.Id).Unwrap();
        if (ticket.InviteId is not null)
        {
            var invite = await LoadInvite(ticket.InviteId, ct);
            if (invite is null)
            {
                return Error.NotFound(ticket.InviteId, "Invite");
            }

            if (!invite.Deleted)
            {
                var err = await AddPermissions(
                    accountId,
                    invite.Permissions.Select(p => (Hrib.Parse(p.Key).Unwrap(), p.Value.Permission)),
                    ct
                );

                if (err.HasErrors)
                {
                    return err.Errors;
                }

                db.Events.KafeAppend(invite.Id, new InviteAccepted(invite.Id));
                await db.SaveChangesAsync(ct);
            }
        }

        if (shouldWaitForDaemon)
        {
            if (!await db.TryWaitForEntityPermissions(accountId, ct))
            {
                logger.LogWarning("Failed to wait for permissions to update within timeout.");
            }
        }

        account = await db.Events.KafeAggregateStream<AccountInfo>(accountId, token: ct);
        if (account is null)
        {
            return Error.NotFound(accountId);
        }

        return account;
    }

    public async Task<Err<InviteInfo>> UpsertInvite(
        InviteInfo invite,
        Hrib inviterAccountId,
        CancellationToken ct = default
    )
    {
        var idResult = Hrib.Parse(invite.Id);
        if (idResult.HasErrors)
        {
            return idResult.Errors;
        }

        var id = idResult.Value;
        if (id.IsInvalid)
        {
            return Error.InvalidValue("The invite ID");
        }

        if (!IsValidEmailAddress(invite.EmailAddress))
        {
            return Error.InvalidValue("The email address is invalid.");
        }

        db.LastModifiedBy = inviterAccountId.ToString();

        if (id.IsEmpty)
        {
            var existing = await FindInviteByEmail(invite.EmailAddress, ct);
            IEventStream<InviteInfo>? eventStream = null;
            if (existing is not null)
            {
                id = Hrib.Parse(existing.Id).Unwrap();
                eventStream = await db.Events.FetchForExclusiveWriting<InviteInfo>(id.ToString(), ct);
            }

            if (existing is null || eventStream?.Aggregate?.Deleted == true)
            {
                id = Hrib.Create();
                db.Events.KafeStartStream<InviteInfo>(
                    id,
                    new InviteCreated(
                        InviteId: id.ToString(),
                        CreationMethod: CreationMethod.Api,
                        EmailAddress: invite.EmailAddress,
                        PreferredCulture: invite.PreferredCulture
                    )
                );
            }
        }

        foreach (var permission in invite.Permissions)
        {
            db.Events.KafeAppend(
                id,
                new InvitePermissionSet(
                    InviteId: id.ToString(),
                    Permission: permission.Value.Permission,
                    EntityId: permission.Value.EntityId
                )
            );
        }

        await db.SaveChangesAsync(ct);
        return await db.Events.KafeAggregateRequiredStream<InviteInfo>(id, token: ct);
    }

    public string EncodeLoginTicketId(Guid id)
    {
        var protectedBytes = dataProtector.Protect(id.ToByteArray());
        return WebEncoders.Base64UrlEncode(protectedBytes);
    }

    public Guid? DecodeLoginTicketId(string protectedId)
    {
        try
        {
            var unprotectedBytes = dataProtector.Unprotect(WebEncoders.Base64UrlDecode(protectedId));
            return new Guid(unprotectedBytes);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool IsValidEmailAddress(string emailAddress)
    {
        return MailboxAddress.TryParse(emailAddress, out var address)
            && address.Address == emailAddress;
    }
}
