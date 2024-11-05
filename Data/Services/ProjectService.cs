using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
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

    public ProjectService(
        IDocumentSession db,
        AccountService accountService,
        ArtifactService artifactService)
    {
        this.db = db;
        this.accountService = accountService;
        this.artifactService = artifactService;
    }

    public async Task<ProjectInfo> Create(
        string projectGroupId,
        LocalizedString name,
        LocalizedString? description,
        LocalizedString? genre,
        Hrib? ownerId,
        Hrib? id = null,
        CancellationToken token = default)
    {
        var group = await db.LoadAsync<ProjectGroupInfo>(projectGroupId, token);
        if (group is null)
        {
            throw new ArgumentOutOfRangeException($"Project group '{projectGroupId}' does not exist.");
        }

        if (!group.IsOpen)
        {
            throw new ArgumentException($"Project group '{projectGroupId}' is not open.");
        }

        var projectId = (id ?? Hrib.Create()).ToString();

        var created = new ProjectCreated(
            ProjectId: projectId,
            CreationMethod: CreationMethod.Api,
            ProjectGroupId: projectGroupId,
            Name: name);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: projectId,
            Name: name,
            Description: description,
            Genre: genre);

        db.Events.KafeStartStream<ProjectInfo>(created.ProjectId, created, infoChanged);
        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                [((Hrib)created.ProjectId, Permission.Read | Permission.Write | Permission.Append)],
                token);
        }

        var project = await db.Events.AggregateStreamAsync<ProjectInfo>(created.ProjectId, token: token);
        if (project is null)
        {
            throw new InvalidOperationException($"Could not persist a project with id '{created.ProjectId}'.");
        }

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

    public async Task<Err<bool>> Edit(ProjectInfo modified, CancellationToken token = default)
    {
        var @old = await Load(modified.Id, token);
        if (@old is null)
        {
            return Error.NotFound(modified.Id);
        }

        if (LocalizedString.IsTooLong(modified.Name, NameMaxLength))
        {
            return new Error("Name is too long.");
        }

        if (LocalizedString.IsTooLong(modified.Genre, GenreMaxLength))
        {
            return new Error("Genre is too long.");
        }

        if (LocalizedString.IsTooLong(modified.Description, DescriptionMaxLength))
        {
            return new Error("Description is too long.");
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(modified.Id, token);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: modified.Id,
            Name: (LocalizedString)@old.Name != modified.Name ? modified.Name : null,
            Description: (LocalizedString?)@old.Description != modified.Description ? modified.Description : null,
            Genre: (LocalizedString?)@old.Genre != modified.Genre ? modified.Genre : null);
        if (infoChanged.Name is not null || infoChanged.Description is not null || infoChanged.Genre is not null)
        {
            eventStream.AppendOne(infoChanged);
        }

        var authorsRemoved = @old.Authors.Except(modified.Authors)
            .Select(a => new ProjectAuthorRemoved(modified.Id, a.Id, a.Kind, a.Roles));
        eventStream.AppendMany(authorsRemoved);
        var authorsAdded = modified.Authors.Except(@old.Authors)
            .Select(a => new ProjectAuthorAdded(modified.Id, a.Id, a.Kind, a.Roles));
        eventStream.AppendMany(authorsAdded);

        var artifactsRemoved = @old.Artifacts.Except(modified.Artifacts)
            .Select(a => new ProjectArtifactRemoved(modified.Id, a.Id));
        eventStream.AppendMany(artifactsRemoved);
        var artifactsAdded = modified.Artifacts.Except(@old.Artifacts)
            .Select(a => new ProjectArtifactAdded(modified.Id, a.Id, a.BlueprintSlot));
        eventStream.AppendMany(artifactsAdded);

        await db.SaveChangesAsync(token);
        return true;
    }

    public record ProjectFilter(
        Hrib? ProjectGroupId = null,
        Hrib? AccessingAccountId = null
    );

    public async Task<ImmutableArray<ProjectInfo>> List(ProjectFilter? filter = null, CancellationToken token = default)
    {
        filter ??= new ProjectFilter();

        var query = db.Query<ProjectInfo>();

        if (filter.AccessingAccountId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .WhereAccountHasPermission(Permission.Read, filter.AccessingAccountId);
        }

        if (filter.ProjectGroupId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .Where(e => e.ProjectGroupId == filter.ProjectGroupId.ToString());
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

    public async Task<ImmutableArray<ProjectInfo>> LoadMany(ImmutableArray<Hrib> ids, CancellationToken token = default)
    {
        return [.. (await db.LoadManyAsync<ProjectInfo>(token, ids.Select(id => id.ToString())))];
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
}
