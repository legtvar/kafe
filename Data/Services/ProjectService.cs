using Kafe.Common;
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
    private readonly IDocumentSession db;
    private readonly AccountService accountService;
    private readonly ArtifactService artifactService;
    private readonly EntityMetadataProvider entityMetadataProvider;

    public ProjectService(
        IDocumentSession db,
        AccountService accountService,
        ArtifactService artifactService,
        EntityMetadataProvider entityMetadataProvider)
    {
        this.db = db;
        this.accountService = accountService;
        this.artifactService = artifactService;
        this.entityMetadataProvider = entityMetadataProvider;
    }

    public async Task<Err<ProjectInfo>> Create(
        ProjectInfo @new,
        Hrib? ownerId = null,
        CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var group = await db.KafeLoadAsync<ProjectGroupInfo>(@new.ProjectGroupId, token);
        if (group.HasErrors)
        {
            return group.Errors;
        }

        if (!group.Value.IsOpen)
        {
            return new Error($"Project group '{@new.ProjectGroupId}' is not open.");
        }

        var projectId = parseResult.Value;
        if (projectId.IsEmpty)
        {
            projectId = Hrib.Create();
        }

        var created = new ProjectCreated(
            ProjectId: projectId.ToString(),
            CreationMethod: CreationMethod.Api,
            ProjectGroupId: @new.ProjectGroupId,
            Name: @new.Name);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: projectId.ToString(),
            Name: @new.Name,
            Description: @new.Description,
            Genre: @new.Genre);

        db.Events.KafeStartStream<ProjectInfo>(created.ProjectId, created, infoChanged);

        if (@new.IsLocked)
        {
            db.Events.KafeAppend(projectId, new ProjectLocked(projectId.ToString()));
        }
        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                [((Hrib)created.ProjectId, Permission.Read | Permission.Write | Permission.Append)],
                token);
        }

        var project = await db.Events.AggregateStreamAsync<ProjectInfo>(created.ProjectId, token: token)
            ?? throw new InvalidOperationException($"Could not persist a project with id '{created.ProjectId}'.");

        return project;
    }

    public async Task AddAuthors(
        Hrib projectId,
        IEnumerable<(string id, ProjectAuthorKind kind, ImmutableArray<string> roles)> authors)
    {
        var authorsAdded = authors
            .Select(a => new ProjectAuthorAdded(
                ProjectId: projectId.ToString(),
                AuthorId: a.id,
                Kind: a.kind,
                Roles: a.roles));
        db.Events.KafeAppend(projectId, authorsAdded);
        await db.SaveChangesAsync();
    }

    public async Task RemoveAuthors(
        Hrib projectId,
        IEnumerable<(string id, ProjectAuthorKind kind, ImmutableArray<string> roles)> authors)
    {
        var authorsAdded = authors
            .Select(a => new ProjectAuthorRemoved(
                ProjectId: projectId.ToString(),
                AuthorId: a.id,
                Kind: a.kind,
                Roles: a.roles));
        db.Events.KafeAppend(projectId, authorsAdded);
        await db.SaveChangesAsync();
    }

    public async Task<Err<bool>> Edit(ProjectInfo @new, CancellationToken token = default)
    {
        var @old = await Load(@new.Id, token);
        if (@old is null)
        {
            return Error.NotFound(@new.Id);
        }

        if (LocalizedString.IsTooLong(@new.Name, NameMaxLength))
        {
            return new Error("Name is too long.");
        }

        if (LocalizedString.IsTooLong(@new.Genre, GenreMaxLength))
        {
            return new Error("Genre is too long.");
        }

        if (LocalizedString.IsTooLong(@new.Description, DescriptionMaxLength))
        {
            return new Error("Description is too long.");
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(@new.Id, token);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: @new.Id,
            Name: (LocalizedString)@old.Name != @new.Name ? @new.Name : null,
            Description: (LocalizedString?)@old.Description != @new.Description ? @new.Description : null,
            Genre: (LocalizedString?)@old.Genre != @new.Genre ? @new.Genre : null);
        if (infoChanged.Name is not null || infoChanged.Description is not null || infoChanged.Genre is not null)
        {
            eventStream.AppendOne(infoChanged);
        }

        if (@new.IsLocked != old.IsLocked)
        {
            eventStream.AppendOne(@new.IsLocked ? new ProjectLocked(old.Id) : new ProjectUnlocked(old.Id));
        }

        var authorsRemoved = @old.Authors.Except(@new.Authors)
            .Select(a => new ProjectAuthorRemoved(@new.Id, a.Id, a.Kind, a.Roles));
        eventStream.AppendMany(authorsRemoved);
        var authorsAdded = @new.Authors.Except(@old.Authors)
            .Select(a => new ProjectAuthorAdded(@new.Id, a.Id, a.Kind, a.Roles));
        eventStream.AppendMany(authorsAdded);

        var artifactsRemoved = @old.Artifacts.Except(@new.Artifacts)
            .Select(a => new ProjectArtifactRemoved(@new.Id, a.Id));
        eventStream.AppendMany(artifactsRemoved);
        var artifactsAdded = @new.Artifacts.Except(@old.Artifacts)
            .Select(a => new ProjectArtifactAdded(@new.Id, a.Id, a.BlueprintSlot));
        eventStream.AppendMany(artifactsAdded);

        await db.SaveChangesAsync(token);
        return true;
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
        return await db.LoadAsync<ProjectInfo>(id.ToString(), token);
        // if (data is null)
        // {
        //     return null;
        // }

        // var group = await db.LoadAsync<ProjectGroupInfo>(data.ProjectGroupId, token);
        // var artifactDetails = await db.LoadManyAsync<ArtifactDetail>(token, data.Artifacts.Select(a => a.Id));
        // var authors = await db.LoadManyAsync<AuthorInfo>(token, data.Authors.Select(a => a.Id));
        // var dto = TransferMaps.ToProjectDetailDto(data) with
        // {
        //     ProjectGroupName = group?.Name ?? Const.UnknownProjectGroup,
        //     Artifacts = artifactDetails.Select(a =>
        //         TransferMaps.ToProjectArtifactDto(a) with
        //         {
        //             BlueprintSlot = data.Artifacts.SingleOrDefault(r => r.Id == a.Id)?.BlueprintSlot
        //         })
        //         .ToImmutableArray(),
        //     Cast = data.Authors.Where(a => a.Kind == ProjectAuthorKind.Cast)
        //             .Select(a => new ProjectAuthorDto(
        //                 Id: a.Id,
        //                 Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
        //                 Roles: a.Roles))
        //             .ToImmutableArray(),
        //     Crew = data.Authors.Where(a => a.Kind == ProjectAuthorKind.Crew)
        //             .Select(a => new ProjectAuthorDto(
        //                 Id: a.Id,
        //                 Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
        //                 Roles: a.Roles))
        //             .ToImmutableArray()
        // };

        // return dto;
    }



    public async Task<ImmutableArray<ProjectInfo>> LoadMany(IEnumerable<Hrib> ids, CancellationToken token = default)
    {
        return (await db.KafeLoadManyAsync<ProjectInfo>(ids.ToImmutableArray(), token)).Unwrap();
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
            return Error.NotFound(projectId, "A project");
        }

        var existingArtifactIds = (await artifactService.LoadMany(artifacts.Select(a => a.id), token))
            .Select(a => a.Id)
            .ToImmutableHashSet();
        if (existingArtifactIds.Count != artifacts.Length)
        {
            return artifacts.Where(a => !existingArtifactIds.Contains(a.id.ToString()))
                .Select(a => Error.NotFound(a.id, "An artifact"))
                .ToImmutableArray();
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
            return Error.NotFound(projectId, "A project");
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
