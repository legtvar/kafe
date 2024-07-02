using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using Marten.Linq.SoftDeletes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class PlaylistService
{
    private readonly IDocumentSession db;
    private readonly OrganizationService organizationService;

    public PlaylistService(
        IDocumentSession db,
        OrganizationService organizationService)
    {
        this.db = db;
        this.organizationService = organizationService;
    }

    public record PlaylistFilter(
        Hrib? AccessingAccountId
    );

    public async Task<ImmutableArray<PlaylistInfo>> List(
        PlaylistFilter? filter = null,
        CancellationToken token = default)
    {
        var query = db.Query<PlaylistInfo>();

        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<PlaylistInfo>)query
                .Where(e => e.MatchesSql(
                    $"({SqlFunctions.GetPlaylistPerms}(data ->> 'Id', ?) & ?) != 0",
                    filter.AccessingAccountId.ToString(),
                    (int)Permission.Read));
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
    }

    public async Task<PlaylistInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<PlaylistInfo>(id.ToString(), token);
    }

    public async Task<ImmutableArray<PlaylistInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<PlaylistInfo>(token, ids.Select(i => (string)i)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }

    public async Task<Err<PlaylistInfo>> Create(
        PlaylistInfo @new,
        CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var organization = await organizationService.Load(@new.OrganizationId, token);
        if (organization is null)
        {
            return Error.NotFound(@new.OrganizationId, "An organization");
        }

        var id = parseResult.Value;
        if (id == Hrib.Empty)
        {
            id = Hrib.Create();
        }

        var created = new PlaylistCreated(
            PlaylistId: id.ToString(),
            CreationMethod: @new.CreationMethod is not CreationMethod.Unknown
                ? @new.CreationMethod
                : CreationMethod.Api,
            OrganizationId: @new.OrganizationId,
            Name: @new.Name);
        db.Events.KafeStartStream<PlaylistInfo>(id, created);

        if (@new.GlobalPermissions != Permission.None || @new.Description is not null)
        {
            var changed = new PlaylistInfoChanged(
                PlaylistId: id.ToString(),
                Description: @new.Description,
                GlobalPermissions: @new.GlobalPermissions);
            db.Events.Append(id.ToString(), changed);
        }

        if (!@new.EntryIds.IsDefaultOrEmpty)
        {
            var entriesSet = new PlaylistEntriesSet(
                PlaylistId: id.ToString(),
                EntryIds: @new.EntryIds);
            db.Events.Append(id.ToString(), entriesSet);
        }

        // TODO: Check all entries exist before putting them in

        await db.SaveChangesAsync(token);
        var playlist = await db.Events.AggregateStreamAsync<PlaylistInfo>(id.ToString(), token: token)
            ?? throw new InvalidOperationException($"Could not persist a playlist with id '{id}'.");
        return playlist;
    }

    public async Task<Err<PlaylistInfo>> Edit(
        PlaylistInfo @new,
        CancellationToken token = default)
    {
        var old = await Load(@new.Id, token);
        if (old is null)
        {
            return Error.NotFound(@new.Id, "A playlist");
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<PlaylistInfo>(@new.Id, token);
        var infoChanged = new PlaylistInfoChanged(
            PlaylistId: @new.Id,
            Name: @new.Name,
            Description: @new.Description,
            GlobalPermissions: @new.GlobalPermissions);
        if (infoChanged.Name is not null
            || infoChanged.Description is not null
            || infoChanged.GlobalPermissions is not null)
        {
            eventStream.AppendOne(infoChanged);
        }

        if (@new.EntryIds.SequenceEqual(old.EntryIds))
        {
            eventStream.AppendOne(new PlaylistEntriesSet(
                PlaylistId: @new.Id,
                EntryIds: old.EntryIds));
        }

        if (@new.OrganizationId != old.OrganizationId)
        {
            eventStream.AppendOne(new PlaylistMovedToOrganization(
                PlaylistId: @new.Id,
                OrganizationId: @new.OrganizationId
            ));
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<PlaylistInfo>(@old.Id, token: token);
    }
}
