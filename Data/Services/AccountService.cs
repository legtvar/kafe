using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public async Task<AccountInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<AccountInfo>(id.Value, token: token);
    }

    public async Task<AccountInfo?> FindByEmail(string emailAddress, CancellationToken token = default)
    {
        return await db.Query<AccountInfo>()
            .Where(a => a.EmailAddress == emailAddress)
            .SingleOrDefaultAsync(token: token);
    }

    public record AccountFilter(
        ImmutableDictionary<string, Permission>? Permissions = default
    );

    public async Task<ImmutableArray<AccountInfo>> List(AccountFilter? filter = null, CancellationToken token = default)
    {
        filter ??= new AccountFilter();

        var query = db.Query<AccountInfo>();
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

    public async Task<AccountInfo> CreateTemporaryAccount(
        string emailAddress,
        string? preferredCulture,
        Hrib? id = null,
        CancellationToken token = default)
    {
        // TODO: Add a "ticket" entity that will be identified by a guid, and will be one-time only instead of these
        //       tokens.

        var account = await db.Query<AccountInfo>().SingleOrDefaultAsync(a => a.EmailAddress == emailAddress, token);
        if (account is null)
        {
            id ??= Hrib.Create();
            var created = new AccountCreated(
                AccountId: id.Value,
                CreationMethod: CreationMethod.Api,
                EmailAddress: emailAddress,
                PreferredCulture: preferredCulture ?? Const.InvariantCultureCode
            );
            var selfPermissionSet = new AccountPermissionSet(id.Value, id.Value, Permission.All);
            db.Events.StartStream<AccountInfo>(id.Value, created, selfPermissionSet);
        }
        else
        {
            id = account.Id;
        }

        var refreshed = new TemporaryAccountRefreshed(
            AccountId: id.Value,
            SecurityStamp: account?.SecurityStamp ?? Guid.NewGuid().ToString()
        );
        db.Events.Append(id.Value, refreshed);
        await db.SaveChangesAsync(token);
        return await db.Events.AggregateStreamAsync<AccountInfo>(id.Value, token: token)
            ?? throw new InvalidOperationException($"Account '{id}' could not be live-aggregated.");
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
        Hrib id,
        IEnumerable<(string id, Permission permission)> permissions,
        CancellationToken token = default)
    {
        // TODO: Find a cheaper way of knowing that an account exists.
        var account = await Load(id, token);
        if (account is null)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<AccountInfo>(account.Id, token);
        eventStream.AppendMany(permissions.Select(c => new AccountPermissionSet(account.Id, c.id, c.permission)));
        await db.SaveChangesAsync(token);
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
        return await db.Events.AggregateStreamAsync<AccountInfo>(id, token: token)
            ?? throw new InvalidOperationException($"Account '{id}' could not be live-aggregated.");
    }
}
