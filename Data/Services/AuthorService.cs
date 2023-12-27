using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using System;
using System.Collections;
using System.Collections.Generic;
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

    public async Task<Err<AuthorInfo>> Create(
        AuthorInfo @new,
        CancellationToken token = default)
    {
        if (!Hrib.TryParse(@new.Id, out var id, out var error))
        {
            return new Error(error);
        }

        if (id == Hrib.Invalid)
        {
            id = Hrib.Create();
        }

        var created = new AuthorCreated(
            AuthorId: id.Value,
            CreationMethod: CreationMethod.Api,
            Name: @new.Name);
        db.Events.StartStream<AuthorInfo>(created.AuthorId, created);

        if (!string.IsNullOrEmpty(@new.Uco)
            || LocalizedString.IsNullOrEmpty(@new.Bio)
            || !string.IsNullOrEmpty(@new.Email)
            || !string.IsNullOrEmpty(@new.Phone)
            || @new.GlobalPermissions != Permission.None
            || !string.IsNullOrEmpty(@new.Name))
        {
            var infoChanged = new AuthorInfoChanged(
                AuthorId: created.AuthorId,
                Name: @new.Name,
                GlobalPermissions: @new.GlobalPermissions,
                Bio: @new.Bio,
                Uco: @new.Uco,
                Email: @new.Email,
                Phone: @new.Phone);
            db.Events.Append(created.AuthorId, infoChanged);
        }

        await db.SaveChangesAsync(token);

        // NB: perform a live aggregation because the DB might still have outdated data
        return await db.Events.AggregateStreamAsync<AuthorInfo>(created.AuthorId, token: token)
            ?? throw new InvalidOperationException($"Could not persist an author with id '{created.AuthorId}'.");
    }

    public record AuthorFilter(
        Hrib? AccessingAccountId = null,
        string? Name = null,
        string? Email = null,
        string? Uco = null,
        string? Phone = null
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

        if (filter?.Name is not null)
        {
            query = (IMartenQueryable<AuthorInfo>)query
                .Where(a => a.Name == filter.Name);
        }

        if (filter?.Email is not null)
        {
            query = (IMartenQueryable<AuthorInfo>)query
                .Where(a => a.Name == filter.Email);
        }

        if (filter?.Uco is not null)
        {
            query = (IMartenQueryable<AuthorInfo>)query
                .Where(a => a.Name == filter.Uco);
        }

        if (filter?.Phone is not null)
        {
            query = (IMartenQueryable<AuthorInfo>)query
                .Where(a => a.Name == filter.Phone);
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
    }

    public async Task<AuthorInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<AuthorInfo>(id.Value, token);
    }

    public async Task<ImmutableArray<AuthorInfo>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default)
    {
        return (await db.LoadManyAsync<AuthorInfo>(token, ids.Select(i => i.Value)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }
}
