﻿using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class AuthorService
{
    private readonly IDocumentSession db;
    private readonly AccountService accountService;

    public AuthorService(
        IDocumentSession db,
        AccountService accountService)
    {
        this.db = db;
        this.accountService = accountService;
    }

    public async Task<AuthorInfo> Create(
        string name,
        LocalizedString? bio,
        string? uco,
        string? email,
        string? phone,
        Hrib? ownerId,
        CancellationToken token = default)
    {
        var created = new AuthorCreated(
            AuthorId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            Name: name);
        db.Events.StartStream<AuthorInfo>(created.AuthorId, created);
        if (!string.IsNullOrEmpty(uco)
            || LocalizedString.IsNullOrEmpty(bio)
            || !string.IsNullOrEmpty(email)
            || !string.IsNullOrEmpty(phone))
        {
            var infoChanged = new AuthorInfoChanged(
                AuthorId: created.AuthorId,
                Bio: bio,
                Uco: uco,
                Email: email,
                Phone: phone);
            db.Events.Append(created.AuthorId, infoChanged);
        }
        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                new [] { (created.AuthorId, Permission.All) },
                token);
        }

        var author = await db.Events.AggregateStreamAsync<AuthorInfo>(created.AuthorId, token: token);
        if (author is null)
        {
            throw new InvalidOperationException($"Could not persist an author with id '{created.AuthorId}'.");
        }

        return author;
    }

    public record AuthorFilter(
        Hrib? AccessingAccountId = null
    );

    public async Task<ImmutableArray<AuthorInfo>> List(AuthorFilter? filter = null, CancellationToken token = default)
    {
        var query = db.Query<AuthorInfo>();
        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<AuthorInfo>)query
                .Where(e => e.MatchesSql(
                    $"({SqlFunctions.GetAuthorPerms}(data ->> 'Id', ?) & ?) != 0",
                    filter.AccessingAccountId.Value,
                    (int)Permission.Read));
        }
        
        return (await query.ToListAsync(token)).ToImmutableArray();
    }

    public async Task<AuthorInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<AuthorInfo>(id, token);
    }
}
