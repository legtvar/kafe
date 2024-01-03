using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class AccountService
{
    public static readonly TimeSpan ConfirmationTokenExpiration = TimeSpan.FromHours(24);
    private readonly IDocumentSession db;

    public const string PreferredUsernameClaim = "preferred_username";

    public AccountService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<AccountInfo?> Load(
        Hrib id,
        CancellationToken token = default)
    {
        return (await db.KafeLoadAsync<AccountInfo>(id, token: token)).GetValueOrDefault();
    }

    public async Task<AccountInfo?> FindByEmail(string emailAddress, CancellationToken token = default)
    {
        return await db.Query<AccountInfo>()
            .Where(a => a.EmailAddress == emailAddress)
            .SingleOrDefaultAsync(token: token);
    }

    public async Task<Err<AccountInfo>> Create(AccountInfo @new, CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var id = parseResult.Value;
        if (id == Hrib.Invalid)
        {
            id = Hrib.Create();
        }

        var created = new AccountCreated(
            AccountId: id.ToString(),
            CreationMethod: @new.CreationMethod,
            EmailAddress: @new.EmailAddress,
            PreferredCulture: @new.PreferredCulture
        );
        var selfPermissionSet = new AccountPermissionSet(id.Value, id.Value, Permission.All);
        db.Events.KafeStartStream<AccountInfo>(id, created, selfPermissionSet);

        switch (@new.Kind)
        {
            case AccountKind.Temporary:
                RefreshTemporaryAccountCore(id.ToString());
                break;
            case AccountKind.External:
                if (string.IsNullOrEmpty(@new.IdentityProvider))
                {
                    return Error.MissingValue(nameof(@new.IdentityProvider));
                }

                var associated = new ExternalAccountAssociated(
                    AccountId: id.ToString(),
                    IdentityProvider: @new.IdentityProvider,
                    Name: @new.Name,
                    Uco: @new.Uco);
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
                Phone: @new.Phone);
            db.Events.KafeAppend(id, infoChanged);
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<AccountInfo>(id, token: token);
    }

    public async Task<Err<AccountInfo>> Edit(AccountInfo modified, CancellationToken token = default)
    {
        var old = await Load(modified.Id, token);
        if (old is null)
        {
            return Error.NotFound(modified.Id, "An account");
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
            db.Events.KafeAppend(old.Id, new AccountPermissionSet(
                AccountId: old.Id,
                EntityId: changedPermission.Key,
                Permission: changedPermission.Value
            ));
        }

        var removedPermissions = old.Permissions.Keys.Except(modified.Permissions.Keys);
        foreach (var removedPermission in removedPermissions)
        {
            hasChanged = true;
            db.Events.KafeAppend(old.Id, new AccountPermissionUnset(
                AccountId: old.Id,
                EntityId: removedPermission
            ));
        }

        if (!hasChanged)
        {
            return Error.Unmodified(old.Id, "An account");
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<AccountInfo>(old.Id, token: token);
    }

    public record AccountFilter(
        string? Uco = null,
        ImmutableDictionary<string, Permission>? Permissions = default
    );

    public async Task<ImmutableArray<AccountInfo>> List(AccountFilter? filter = null, CancellationToken token = default)
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
$@"TRUE = ALL(
    SELECT (data -> 'Permissions' -> entity_id)::int & expected = expected
    FROM (VALUES {permValues}) as expected_perms (entity_id, expected)
)"));
        }

        var results = await query.ToListAsync();
        return results.ToImmutableArray();
    }

    public async Task<Err<AccountInfo>> CreateOrRefreshTemporaryAccount(
        string emailAddress,
        string? preferredCulture,
        Hrib? id = null,
        CancellationToken token = default)
    {
        // TODO: Add a "ticket" entity that will be identified by a guid, and will be one-time only instead of these
        //       tokens.

        if (!IsValidEmailAddress(emailAddress))
        {
            throw new ArgumentException(
                "The provided email address does not have a valid format.",
                nameof(emailAddress));
        }

        var account = await FindByEmail(emailAddress, token);
        if (account is null)
        {
            return await Create(AccountInfo.Invalid with
            {
                Id = (id ?? Hrib.Create()).ToString(),
                Kind = AccountKind.Temporary,
                EmailAddress = emailAddress,
                PreferredCulture = preferredCulture ?? Const.InvariantCultureCode
            }, token);
        }
        else
        {
            RefreshTemporaryAccountCore(emailAddress);
            await db.SaveChangesAsync(token);
            return await db.Events.KafeAggregateRequiredStream<AccountInfo>(account.Id, token: token);
        }
    }

    private void RefreshTemporaryAccountCore(Hrib id)
    {
        var refreshed = new TemporaryAccountRefreshed(
            AccountId: id.Value,
            SecurityStamp: Guid.NewGuid().ToString()
        );
        db.Events.Append(id.Value, refreshed);
    }

    public async Task<bool> TryConfirmTemporaryAccount(
        Hrib id,
        string securityStamp,
        CancellationToken token = default)
    {
        // TODO: Add a "ticket" entity that will be identified by a guid, and will be one-time only instead of these
        //       tokens.

        var account = await Load(id, token);
        if (account is null)
        {
            // throw new UnauthorizedAccessException("The account does not exist.");
            return false;
        }

        if (account.RefreshedOn + ConfirmationTokenExpiration < DateTimeOffset.UtcNow)
        {
            // var closedExpired = new TemporaryAccountClosed(account.Id);
            // db.Events.Append(account.Id, closedExpired);
            // await db.SaveChangesAsync(token);
            // throw new UnauthorizedAccessException("The token has expired.");
            return false;
        }

        if (account.SecurityStamp != securityStamp)
        {
            // throw new UnauthorizedAccessException("The token has been already used or revoked.");
            return false;
        }

        return true;

        //var closedSuccessfully = new TemporaryAccountClosed(account.Id);
        //db.Events.Append(account.Id, closedSuccessfully);
        //await db.SaveChangesAsync(token);
    }

    /// <summary>
    /// Gives the account the specified capabilities.
    /// </summary>
    public async Task AddPermissions(
        Hrib accountId,
        IEnumerable<(string id, Permission permission)> permissions,
        CancellationToken token = default)
    {
        // TODO: Find a cheaper way of knowing that an account exists.
        var account = await Load(accountId, token);
        if (account is null)
        {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<AccountInfo>(account.Id, token);
        eventStream.AppendMany(permissions.Select(c => new AccountPermissionSet(account.Id, c.id, c.permission)));
        await db.SaveChangesAsync(token);
    }

    public Task AddPermissions(
        Hrib accountId,
        CancellationToken token = default,
        params (string id, Permission permission)[] permissions)
    {
        return AddPermissions(accountId, permissions, token);
    }

    public async Task<Err<AccountInfo>> AssociateExternalAccount(
        ClaimsPrincipal principal,
        CancellationToken token = default)
    {
        var emailClaim = principal.FindFirst(ClaimTypes.Email);
        if (emailClaim is null || string.IsNullOrEmpty(emailClaim.Value))
        {
            return Error.MissingValue("email address");
        }

        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var uco = principal.FindFirst(PreferredUsernameClaim)?.Value;
        var identityProvider = emailClaim.Issuer;

        var existing = await FindByEmail(emailClaim.Value, token);
        if (existing is not null
            && existing.Kind == AccountKind.External
            && existing.IdentityProvider == identityProvider)
        {
            return existing;
        }

        var id = existing?.Id ?? Hrib.Create().Value;
        var associated = new ExternalAccountAssociated(
            AccountId: id,
            IdentityProvider: identityProvider,
            Name: name,
            Uco: uco);

        if (existing is null)
        {
            var created = new AccountCreated(
                AccountId: id,
                CreationMethod: CreationMethod.Api,
                EmailAddress: emailClaim.Value,
                PreferredCulture: Const.InvariantCultureCode
            );
            db.Events.StartStream<AccountInfo>(id, created, associated);
        }
        else
        {
            db.Events.Append(id, associated);
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<AccountInfo>(id, token: token);
    }

    public static bool IsValidEmailAddress(string emailAddress)
    {
        return MailboxAddress.TryParse(emailAddress, out var address)
            && address.Address == emailAddress;
    }
}
