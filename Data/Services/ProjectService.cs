using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
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
            ProjectId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            ProjectGroupId: projectGroupId,
            Name: name,
            Visibility: Visibility.Private);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: Hrib.Create(),
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
                ProjectId: projectId,
                AuthorId: a.id,
                Kind: a.kind,
                Roles: a.roles));
        db.Events.Append(projectId, authorsAdded);
        await db.SaveChangesAsync();
    }
    
    public async Task RemoveAuthors(
        Hrib projectId,
        IEnumerable<(string id, ProjectAuthorKind kind, ImmutableArray<string> roles)> authors)
    {
        var authorsAdded = authors
            .Select(a => new ProjectAuthorRemoved(
                ProjectId: projectId,
                AuthorId: a.id,
                Kind: a.kind,
                Roles: a.roles));
        db.Events.Append(projectId, authorsAdded);
        await db.SaveChangesAsync();
    }

    public async Task Edit(ProjectEditDto dto, CancellationToken token = default)
    {
        var project = await Load(dto.Id, token);
        if (project is null)
        {
            throw new ArgumentOutOfRangeException(nameof(dto));
        }

        if (LocalizedString.IsTooLong(dto.Name, NameMaxLength))
        {
            throw new ArgumentException("Name is too long.", nameof(dto));
        }

        if (LocalizedString.IsTooLong(dto.Genre, GenreMaxLength))
        {
            throw new ArgumentException("Genre is too long.", nameof(dto));
        }

        if (LocalizedString.IsTooLong(dto.Description, DescriptionMaxLength))
        {
            throw new ArgumentException("Description is too long.", nameof(dto));
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(dto.Id, token);

        if (!LocalizedString.IsNullOrEmpty(dto.Name)
            || !LocalizedString.IsNullOrEmpty(dto.Description)
            || !LocalizedString.IsNullOrEmpty(dto.Genre))
        {
            var infoChanged = new ProjectInfoChanged(
                ProjectId: dto.Id,
                Name: dto.Name,
                Description: dto.Description,
                Genre: dto.Genre);
            eventStream.AppendOne(infoChanged);
        }

        // TODO: Proper author diffing
        // TODO: Send "Authors" instead of the stupid "Cast" and "Crew" to the client
        //       (change in dto)

        if (dto.Crew is not null)
        {
            var authorsRemoved = project.Crew.Select(c => c.Id)
                .Select(i => new ProjectAuthorRemoved(dto.Id, i, ProjectAuthorKind.Crew));
            eventStream.AppendMany(authorsRemoved);
        }

        if (dto.Cast is not null)
        {
            var authorsRemoved = project.Cast.Select(c => c.Id)
                .Select(i => new ProjectAuthorRemoved(dto.Id, i, ProjectAuthorKind.Cast));
            eventStream.AppendMany(authorsRemoved);
        }

        if (dto.Crew is not null || dto.Cast is not null)
        {
            var newAuthors = await GetProjectAuthors(
                dto.Crew ?? Enumerable.Empty<ProjectCreationAuthorDto>(),
                dto.Cast ?? Enumerable.Empty<ProjectCreationAuthorDto>(),
                token);
            var authorsAdded = newAuthors
                .Select(a => new ProjectAuthorAdded(
                    ProjectId: dto.Id,
                    AuthorId: a.Id,
                    Kind: a.Kind,
                    Roles: a.Roles));
            eventStream.AppendMany(authorsAdded);
        }

        if (dto.Artifacts is not null)
        {
            foreach (var artifact in project.Artifacts)
            {
                eventStream.AppendOne(new ProjectArtifactRemoved(dto.Id, artifact.Id));
            }

            foreach (var artifact in dto.Artifacts)
            {
                var artifactInfo = await db.LoadAsync<ArtifactInfo>(artifact.Id, token);
                if (artifactInfo is null)
                {
                    throw new IndexOutOfRangeException($"Artifact '{artifact.Id}' does not exist.");
                }

                if (artifact.BlueprintSlot is not null
                    && !project.Blueprint.ArtifactBlueprints.TryGetValue(artifact.BlueprintSlot, out var _))
                {
                    throw new IndexOutOfRangeException($"BlueprintSlot '{artifact.BlueprintSlot}' is not defined.");
                }

                eventStream.AppendOne(new ProjectArtifactAdded(dto.Id, artifact.Id, artifact.BlueprintSlot));
            }
        }

        await db.SaveChangesAsync(token);
    }

    public async Task<ImmutableArray<ProjectInfo>> List(CancellationToken token = default)
    {
        return (await db.Query<ProjectInfo>().ToListAsync(token)).ToImmutableArray();
    }

    public async Task<ProjectInfo?> Load(Hrib id, CancellationToken token = default)
    {
        var data = await db.LoadAsync<ProjectInfo>(id, token);
        if (data is null)
        {
            return null;
        }

        var group = await db.LoadAsync<ProjectGroupInfo>(data.ProjectGroupId, token);
        var artifactDetails = await db.LoadManyAsync<ArtifactDetail>(token, data.Artifacts.Select(a => a.Id));
        var authors = await db.LoadManyAsync<AuthorInfo>(token, data.Authors.Select(a => a.Id));
        var dto = TransferMaps.ToProjectDetailDto(data) with
        {
            ProjectGroupName = group?.Name ?? Const.UnknownProjectGroup,
            Artifacts = artifactDetails.Select(a =>
                TransferMaps.ToProjectArtifactDto(a) with
                {
                    BlueprintSlot = data.Artifacts.SingleOrDefault(r => r.Id == a.Id)?.BlueprintSlot
                })
                .ToImmutableArray(),
            Cast = data.Authors.Where(a => a.Kind == ProjectAuthorKind.Cast)
                    .Select(a => new ProjectAuthorDto(
                        Id: a.Id,
                        Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
                        Roles: a.Roles))
                    .ToImmutableArray(),
            Crew = data.Authors.Where(a => a.Kind == ProjectAuthorKind.Crew)
                    .Select(a => new ProjectAuthorDto(
                        Id: a.Id,
                        Name: authors.SingleOrDefault(e => e?.Id == a.Id)?.Name ?? (string)Const.UnknownAuthor,
                        Roles: a.Roles))
                    .ToImmutableArray()
        };

        return dto;
    }

    public async Task<ImmutableArray<ProjectInfo>> LoadMany(ImmutableArray<Hrib> ids, CancellationToken token = default)
    {
        var results = await Task.WhenAll(ids.Map(i => db.Events.AggregateStreamAsync<ProjectInfo>(i, token: token)));
        return results.Where(r => r is not null)
            .ToImmutableArray()!;
    }

    private async Task<ImmutableArray<ProjectAuthorInfo>> GetProjectAuthors(
        IEnumerable<ProjectCreationAuthorDto> crew,
        IEnumerable<ProjectCreationAuthorDto> cast,
        CancellationToken token = default)
    {
        var authors = crew.Select(c => new ProjectAuthorInfo(
            Id: c.Id,
            Kind: ProjectAuthorKind.Crew,
            Roles: c.Roles.IsDefaultOrEmpty
                ? ImmutableArray<string>.Empty
                : c.Roles))
            .Concat(cast.Select(c => new ProjectAuthorInfo(
                Id: c.Id,
                Kind: ProjectAuthorKind.Cast,
                Roles: c.Roles.IsDefaultOrEmpty
                    ? ImmutableArray<string>.Empty
                    : c.Roles)))
            .ToImmutableArray();

        var ids = authors.Select(d => d.Id).ToImmutableArray();

        var infos = (await db.Query<AuthorInfo>()
            .WhereCanRead(userProvider)
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(token))
            .ToImmutableDictionary(a => a.Id);

        foreach (var author in authors)
        {
            if (!infos.TryGetValue(author.Id, out _))
            {
                throw new IndexOutOfRangeException($"Author '{author.Id}' either doesn't exist or is not accessible.");
            }
        }

        return authors;
    }
}
