using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class AccountService
{
    public static readonly TimeSpan ConfirmationTokenExpiration = TimeSpan.FromHours(24);
    private readonly IDocumentSession db;
    private readonly IHostEnvironment environment;

    public AccountService(
        IDocumentSession db,
        IHostEnvironment environment)
    {
        this.db = db;
        this.environment = environment;
    }

    public async Task<AccountInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.Events.AggregateStreamAsync<AccountInfo>(id, token: token);
    }
    
    public async Task<AccountInfo?> Load(string emailAddress, CancellationToken token = default)
    {
        return await db.Query<AccountInfo>()
            .Where(a => a.EmailAddress == emailAddress)
            .SingleOrDefaultAsync(token: token);
    }

    public async Task<AccountInfo> CreateTemporaryAccount(
        string emailAddress,
        string? preferredCulture,
        CancellationToken token = default)
    {
        // TODO: Add a "ticket" entity that will be identified by a guid, and will be one-time only instead of these
        //       tokens.

        var account = await db.Query<AccountInfo>().SingleOrDefaultAsync(a => a.EmailAddress == emailAddress, token);
        Hrib? id;
        if (account is null)
        {
            id = Hrib.Create();
            var created = new TemporaryAccountCreated(
                AccountId: id,
                CreationMethod: CreationMethod.Api,
                EmailAddress: emailAddress,
                PreferredCulture: preferredCulture ?? Const.InvariantCultureCode
            );
            var selfPermissionSet = new AccountPermissionSet(id, id, Permission.All);
            db.Events.StartStream<AccountInfo>(id, created, selfPermissionSet);
        }
        else
        {
            id = account.Id;
        }

        var refreshed = new TemporaryAccountRefreshed(
            AccountId: id,
            SecurityStamp: account?.SecurityStamp ?? Guid.NewGuid().ToString()
        );
        db.Events.Append(id, refreshed);
        await db.SaveChangesAsync(token);
        account = await db.Events.AggregateStreamAsync<AccountInfo>(id, token: token);
        if (account is null)
        {
            throw new InvalidOperationException($"Account '{id}' could no be found.");
        }
        return account;
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

    public async Task<ImmutableArray<AccountInfo>> List(CancellationToken token = default)
    {
        return (await db.Query<AccountInfo>().ToListAsync(token)).ToImmutableArray();
    }
}
