﻿using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class PlaylistService
{
    private readonly IKafeDocumentSession db;
    private readonly OrganizationService organizationService;
    private readonly ArtifactService artifactService;
    private readonly EntityMetadataProvider entityMetadataProvider;
    private readonly DiagnosticFactory diagnosticFactory;

    public PlaylistService(
        IKafeDocumentSession db,
        OrganizationService organizationService,
        ArtifactService artifactService,
        EntityMetadataProvider entityMetadataProvider,
        DiagnosticFactory diagnosticFactory
    )
    {
        this.db = db;
        this.organizationService = organizationService;
        this.artifactService = artifactService;
        this.entityMetadataProvider = entityMetadataProvider;
        this.diagnosticFactory = diagnosticFactory;
    }

    /// <summary>
    /// Filter of playlists.
    /// </summary>
    /// <param name="AccessingAccountId">
    /// <list type="bullet">
    /// <item> If null, doesn't filter by account access at all.</item>
    /// <item>
    ///     If <see cref="Hrib.Empty"/> assumes the account is an anonymous user
    ///     and filters only by global permissions.
    /// </item>
    /// <item> If <see cref="Hrib.Invalid"/>, throws an exception. </item>
    /// </list>
    /// </param>
    public record PlaylistFilter(
        Hrib? AccessingAccountId,
        Hrib? OrganizationId
    );

    public async Task<ImmutableArray<PlaylistInfo>> List(
        PlaylistFilter? filter = null,
        string? sort = null,
        CancellationToken token = default)
    {
        var query = db.Query<PlaylistInfo>();
        if (filter?.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<PlaylistInfo>)query
                .WhereAccountHasPermission(
                    db.DocumentStore.Options.Schema,
                    Permission.Read,
                    filter.AccessingAccountId);
        }

        if (filter?.OrganizationId is not null)
        {
            query = (IMartenQueryable<PlaylistInfo>)query
                .Where(p => p.OrganizationId == filter.OrganizationId.ToString());
        }

        if (!string.IsNullOrEmpty(sort))
        {
            query = (IMartenQueryable<PlaylistInfo>)query.OrderBySortString(entityMetadataProvider, sort);
        }

        var result = (await query.ToListAsync(token)).ToImmutableArray();
        return result;
    }

    public async Task<PlaylistInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return (await db.LoadAsync<PlaylistInfo>(id, token)).GetValueOrDefault();
    }

    public async Task<ImmutableArray<PlaylistInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<PlaylistInfo>([.. ids], token)).Unwrap();
    }

    public async Task<Err<PlaylistInfo>> Create(
        PlaylistInfo @new,
        CancellationToken token = default)
    {
        if (!Hrib.TryParse(@new.Id, out var id, out _))
        {
            return diagnosticFactory.FromPayload(new BadHribDiagnostic(@new.Id));
        }

        var organization = await organizationService.Load(@new.OrganizationId, token);
        if (organization is null)
        {
            return diagnosticFactory.NotFound<OrganizationInfo>(@new.OrganizationId);
        }

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

        if (@new.Description is not null)
        {
            var changed = new PlaylistInfoChanged(
                PlaylistId: id.ToString(),
                Description: @new.Description);
            db.Events.Append(id.ToString(), changed);
        }

        if (@new.GlobalPermissions != Permission.None)
        {
            var globalPermissionChanged = new PlaylistGlobalPermissionsChanged(
                PlaylistId: id.ToString(),
                GlobalPermissions: @new.GlobalPermissions
            );
            db.Events.Append(id.ToString(), globalPermissionChanged);
        }

        if (!@new.EntryIds.IsDefaultOrEmpty)
        {
            var existanceCheck = await CheckArtifactsExist(
                @new.EntryIds.Select(i => (Hrib)i).ToImmutableArray(),
                token);
            if (existanceCheck.Diagnostic is not null)
            {
                return existanceCheck.Diagnostic;
            }

            var entriesSet = new PlaylistEntriesSet(
                PlaylistId: id.ToString(),
                EntryIds: @new.EntryIds);
            db.Events.Append(id.ToString(), entriesSet);
        }

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
            return diagnosticFactory.NotFound<PlaylistInfo>(@new.Id);
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<PlaylistInfo>(@new.Id, token);
        var infoChanged = new PlaylistInfoChanged(
            PlaylistId: @new.Id,
            Name: @new.Name,
            Description: @new.Description);
        if (infoChanged.Name is not null
            || infoChanged.Description is not null)
        {
            eventStream.AppendOne(infoChanged);
        }

        if (@new.GlobalPermissions != Permission.None
            && @new.GlobalPermissions != old.GlobalPermissions)
        {
            var globalPermissionChanged = new PlaylistGlobalPermissionsChanged(
                PlaylistId: @new.Id,
                GlobalPermissions: @new.GlobalPermissions
            );
            eventStream.AppendOne(globalPermissionChanged);
        }

        if (!@new.EntryIds.SequenceEqual(old.EntryIds))
        {
            var existanceCheck = await CheckArtifactsExist(
                @new.EntryIds.Select(i => (Hrib)i).ToImmutableArray(),
                token);
            if (existanceCheck.Diagnostic is not null)
            {
                return existanceCheck.Diagnostic;
            }

            eventStream.AppendOne(new PlaylistEntriesSet(
                PlaylistId: @new.Id,
                EntryIds: @new.EntryIds));
        }

        if (@new.OrganizationId != old.OrganizationId)
        {
            if (!Hrib.TryParse(@new.OrganizationId, out var organizationId, out _)
                || !organizationId.IsValidNonEmpty)
            {
                return diagnosticFactory.ForParameter(
                    nameof(PlaylistInfo.OrganizationId),
                    new BadHribDiagnostic(@new.OrganizationId)
                );
            }

            eventStream.AppendOne(new PlaylistMovedToOrganization(
                PlaylistId: @new.Id,
                OrganizationId: @new.OrganizationId
            ));
        }

        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<PlaylistInfo>(@old.Id, token: token);
    }

    private async Task<Err<bool>> CheckArtifactsExist(
        ImmutableArray<Hrib> artifactIds,
        CancellationToken token = default
    )
    {
        var artifacts = await db.LoadManyAsync<ArtifactInfo>(artifactIds, token);
        if (artifacts.Diagnostic is not null)
        {
            return artifacts.Diagnostic;
        }

        return true;
    }
}
