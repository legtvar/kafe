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

        var created = new ProjectCreated(
            ProjectId: Hrib.Create().Value,
            CreationMethod: CreationMethod.Api,
            ProjectGroupId: projectGroupId,
            Name: name);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: Hrib.Create().Value,
            Name: name,
            Description: description,
            Genre: genre);

        db.Events.StartStream<ProjectInfo>(created.ProjectId, created, infoChanged);
        await db.SaveChangesAsync(token);

        if (ownerId is not null)
        {
            await accountService.AddPermissions(
                ownerId,
                new[] { (created.ProjectId, Permission.All) },
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
                ProjectId: projectId.Value,
                AuthorId: a.id,
                Kind: a.kind,
                Roles: a.roles));
        db.Events.Append(projectId.Value, authorsAdded);
        await db.SaveChangesAsync();
    }

    public async Task RemoveAuthors(
        Hrib projectId,
        IEnumerable<(string id, ProjectAuthorKind kind, ImmutableArray<string> roles)> authors)
    {
        var authorsAdded = authors
            .Select(a => new ProjectAuthorRemoved(
                ProjectId: projectId.Value,
                AuthorId: a.id,
                Kind: a.kind,
                Roles: a.roles));
        db.Events.Append(projectId.Value, authorsAdded);
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
                .Where(e => e.MatchesSql(
                    $"({SqlFunctions.GetProjectPerms}(data ->> 'Id', ?) & ?) != 0",
                    filter.AccessingAccountId.Value,
                    (int)Permission.Read));
        }

        if (filter.ProjectGroupId is not null)
        {
            query = (IMartenQueryable<ProjectInfo>)query
                .Where(e => e.ProjectGroupId == filter.ProjectGroupId.Value);
        }
        var results = (await query.ToListAsync(token)).ToImmutableArray();
        return results;
    }

    public async Task<ProjectInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ProjectInfo>(id.Value, token);
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
        return (await db.LoadManyAsync<ProjectInfo>(token, ids.Cast<string>())).ToImmutableArray();
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
}
