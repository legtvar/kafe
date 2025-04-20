﻿using Kafe.Core.Diagnostics;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Metadata;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public partial class ProjectService
{
    private readonly IKafeDocumentSession db;
    private readonly AccountService accountService;
    private readonly ArtifactService artifactService;
    private readonly AuthorService authorService;
    private readonly EntityMetadataProvider entityMetadataProvider;
    private readonly DiagnosticFactory diagnosticFactory;

    public ProjectService(
        IKafeDocumentSession db,
        AccountService accountService,
        ArtifactService artifactService,
        AuthorService authorService,
        EntityMetadataProvider entityMetadataProvider,
        DiagnosticFactory diagnosticFactory
    )
    {
        this.db = db;
        this.accountService = accountService;
        this.artifactService = artifactService;
        this.authorService = authorService;
        this.entityMetadataProvider = entityMetadataProvider;
        this.diagnosticFactory = diagnosticFactory;
    }

    public async Task<Err<ProjectInfo>> Upsert(
        ProjectInfo project,
        Hrib? ownerId = null,
        ExistingEntityHandling existingEntityHandling = ExistingEntityHandling.Upsert,
        bool shouldOverrideLock = false,
        CancellationToken token = default
    )
    {
        if (!Hrib.TryParse(project.Id, out var id, out _))
        {
            return diagnosticFactory.FromPayload(new BadHribDiagnostic(project.Id));
        }

        if (id.IsEmpty)
        {
            id = Hrib.Create();
        }

        var existing = await Load(id, token);
        if (existing is null && existingEntityHandling == ExistingEntityHandling.Update)
        {
            return diagnosticFactory.NotFound<ProjectInfo>(id);
        }
        else if (existing is not null && existingEntityHandling == ExistingEntityHandling.Insert)
        {
            return diagnosticFactory.AlreadyExists<ProjectInfo>(id);
        }

        if (existing?.IsLocked == true && !shouldOverrideLock)
        {
            return diagnosticFactory.Locked<ProjectInfo>(id);
        }

        if (LocalizedString.IsTooLong(project.Name, NameMaxLength))
        {
            return diagnosticFactory.ForParameter(
                nameof(project.Name),
                new StringTooLongDiagnostic(project.Name, NameMaxLength)
            );
        }

        if (LocalizedString.IsTooLong(project.Genre, GenreMaxLength))
        {
            return diagnosticFactory.ForParameter(
                nameof(project.Genre),
                new StringTooLongDiagnostic(project.Genre!, GenreMaxLength)
            );
        }

        if (LocalizedString.IsTooLong(project.Description, DescriptionMaxLength))
        {
            return diagnosticFactory.ForParameter(
                nameof(project.Description),
                new StringTooLongDiagnostic(project.Name, DescriptionMaxLength)
            );
        }

        if (existing is null)
        {
            var group = await db.LoadAsync<ProjectGroupInfo>((Hrib)project.ProjectGroupId, token);
            if (group.HasError)
            {
                return group.Diagnostic;
            }

            var created = new ProjectCreated(
                ProjectId: id.ToString(),
                CreationMethod: CreationMethod.Api,
                ProjectGroupId: project.ProjectGroupId,
                Name: project.Name);
            db.Events.KafeStartStream<ProjectInfo>(id, created);
            await db.SaveChangesAsync(token);
        }

        existing = await db.Events.KafeAggregateRequiredStream<ProjectInfo>(id, token: token);
        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(id.ToString(), token);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: id.ToString(),
            Name: LocalizedString.MakeOverride(existing.Name, project.Name),
            Description: LocalizedString.MakeOverride(existing.Description, project.Description),
            Genre: LocalizedString.MakeOverride(existing.Genre, project.Genre)
        );
        if (infoChanged.Name is not null || infoChanged.Description is not null || infoChanged.Genre is not null)
        {
            eventStream.AppendOne(infoChanged);
        }

        if (project.IsLocked != existing.IsLocked)
        {
            eventStream.AppendOne(project.IsLocked
                ? new ProjectLocked(id.ToString())
                : new ProjectUnlocked(id.ToString()));
        }

        var authorsRemoved = existing.Authors.Except(project.Authors)
            .Select(a => new ProjectAuthorRemoved(id.ToString(), a.Id, a.Kind, a.Roles));
        eventStream.AppendMany(authorsRemoved);

        var authorsAdded = project.Authors.Except(existing.Authors)
            .Select(a => new ProjectAuthorAdded(id.ToString(), a.Id, a.Kind, a.Roles));
        // TODO: Check the authors not only exist but also the current user may Read them.
        var authorsAddedInfos = await db.LoadManyAsync<AuthorInfo>(
            [.. authorsAdded.Select(a => (Hrib)a.AuthorId)],
            token
        );
        if (authorsAddedInfos.HasError)
        {
            return authorsAddedInfos.Diagnostic;
        }

        eventStream.AppendMany(authorsAdded);

        var artifactsRemoved = existing.Artifacts.Except(project.Artifacts)
            .Select(a => new ProjectArtifactRemoved(id.ToString(), a.Id));
        eventStream.AppendMany(artifactsRemoved);

        var artifactsAdded = project.Artifacts.Except(existing.Artifacts)
            .Select(a => new ProjectArtifactAdded(id.ToString(), a.Id, a.BlueprintSlot));
        // TODO: Check the artifacts not only exist but also the current user may Read them.
        var artifactsAddedInfos = await db.LoadManyAsync<ArtifactInfo>(
            [.. artifactsAdded.Select(a => (Hrib)a.ArtifactId)],
            token
        );
        if (artifactsAddedInfos.HasError)
        {
            return artifactsAddedInfos.Diagnostic;
        }
        eventStream.AppendMany(artifactsAdded);

        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                [(id, Permission.Read | Permission.Write | Permission.Append | Permission.Inspect)],
                token);
        }

        return await db.Events.KafeAggregateRequiredStream<ProjectInfo>(id, token: token);
    }

    /// <summary>
    /// Filter of projects.
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
    public record ProjectFilter(
        Hrib? ProjectGroupId = null,
        Hrib? OrganizationId = null,
        Hrib? AccessingAccountId = null
    );

    public async Task<ImmutableArray<ProjectInfo>> List(
        ProjectFilter? filter = null,
        string? sort = null,
        CancellationToken token = default)
    {
        filter ??= new ProjectFilter();

        var query = db.Query<ProjectInfo>();

        if (filter.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .WhereAccountHasPermission(
                    db.DocumentStore.Options.Schema,
                    Permission.Read,
                    filter.AccessingAccountId);
        }

        if (filter.ProjectGroupId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .Where(e => e.ProjectGroupId == filter.ProjectGroupId.ToString());
        }

        if (filter.OrganizationId is not null)
        {
            var sql =
            $"""
            (
                SELECT g.data ->> 'OrganizationId'
                FROM {db.DocumentStore.Options.Schema.For<ProjectGroupInfo>()} AS g
                WHERE g.id = d.data ->> 'ProjectGroupId'
            ) = ?
            """;
            query = (IMartenQueryable<ProjectInfo>)query
                .Where(p => p.MatchesSql(sql, filter.OrganizationId.ToString()));
        }

        if (!string.IsNullOrEmpty(sort))
        {
            query = (IMartenQueryable<ProjectInfo>)query.OrderBySortString(entityMetadataProvider, sort);
        }

        var results = (await query.ToListAsync(token)).ToImmutableArray();
        return results;
    }

    public async Task<ProjectInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return (await db.LoadAsync<ProjectInfo>(id, token)).GetValueOrDefault();
    }

    public async Task<ImmutableArray<ProjectInfo>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default)
    {
        return (await db.LoadManyAsync<ProjectInfo>([.. ids], token)).Unwrap();
    }

    // private async Task<ImmutableArray<ProjectAuthorInfo>> GetProjectAuthors(
    //     IEnumerable<ProjectCreationAuthorDto> crew,
    //     IEnumerable<ProjectCreationAuthorDto> cast,
    //     CancellationToken token = default)
    // {
    //     var authors = crew.Select(c => new ProjectAuthorInfo(
    //         Id: c.Id,
    //         Kind: ProjectAuthorKind.Crew,
    //         Roles: c.Roles.IsDefaultOrEmpty
    //             ? ImmutableArray<string>.Empty
    //             : c.Roles))
    //         .Concat(cast.Select(c => new ProjectAuthorInfo(
    //             Id: c.Id,
    //             Kind: ProjectAuthorKind.Cast,
    //             Roles: c.Roles.IsDefaultOrEmpty
    //                 ? ImmutableArray<string>.Empty
    //                 : c.Roles)))
    //         .ToImmutableArray();

    //     var ids = authors.Select(d => d.Id).ToImmutableArray();

    //     var infos = (await db.Query<AuthorInfo>()
    //         .WhereCanRead(userProvider)
    //         .Where(a => ids.Contains(a.Id))
    //         .ToListAsync(token))
    //         .ToImmutableDictionary(a => a.Id);

    //     foreach (var author in authors)
    //     {
    //         if (!infos.TryGetValue(author.Id, out _))
    //         {
    //             throw new IndexOutOfRangeException($"Author '{author.Id}' either doesn't exist or is not accessible.");
    //         }
    //     }

    //     return authors;
    // }

    public async Task<Err<ProjectInfo>> AddArtifacts(
        Hrib projectId,
        ImmutableArray<(Hrib id, string? blueprintSlot)> artifacts,
        CancellationToken token = default)
    {
        var project = await Load(projectId, token);
        if (project is null)
        {
            return diagnosticFactory.NotFound<ProjectInfo>(projectId);
        }

        var existingArtifactIds = (await artifactService.LoadMany(artifacts.Select(a => a.id), token))
            .Select(a => a.Id)
            .ToImmutableHashSet();
        if (existingArtifactIds.Count != artifacts.Length)
        {
            return diagnosticFactory.Aggregate(
                artifacts.Where(a => !existingArtifactIds.Contains(a.id.ToString()))
                    .Select(a => diagnosticFactory.NotFound<ArtifactInfo>(a.id))
                );
        }

        var projectStream = await db.Events.FetchForWriting<ProjectInfo>(projectId.ToString(), token);
        foreach (var artifact in artifacts)
        {
            var artifactAdded = new ProjectArtifactAdded(
                ProjectId: projectId.ToString(),
                ArtifactId: artifact.id.ToString(),
                BlueprintSlot: artifact.blueprintSlot?.ToString());
            projectStream.AppendOne(artifactAdded);
        }
        await db.SaveChangesAsync(token);
        return await db.Events.KafeAggregateRequiredStream<ProjectInfo>(projectId, token: token);
    }

    public Task<Err<bool>> Lock(Hrib projectId, CancellationToken token = default)
    {
        return LockUnlock(projectId: projectId, shouldLock: true, token: token);
    }

    public Task<Err<bool>> Unlock(Hrib projectId, CancellationToken token = default)
    {
        return LockUnlock(projectId: projectId, shouldLock: false, token: token);
    }

    private async Task<Err<bool>> LockUnlock(Hrib projectId, bool shouldLock, CancellationToken token = default)
    {
        var stream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(projectId.ToString(), token);
        if (stream is null)
        {
            return diagnosticFactory.NotFound<ProjectInfo>(projectId);
        }

        if (shouldLock && !stream.Aggregate.IsLocked)
        {
            stream.AppendOne(new ProjectLocked(projectId.ToString()));
        }
        else if (!shouldLock && stream.Aggregate.IsLocked)
        {
            stream.AppendOne(new ProjectUnlocked(projectId.ToString()));
        }
        await db.SaveChangesAsync(token);
        return true;
    }
}
