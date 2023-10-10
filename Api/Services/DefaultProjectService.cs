using JasperFx.Core;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public partial class DefaultProjectService : IProjectService
{
    private readonly IDocumentSession db;
    private readonly IUserProvider userProvider;
    private readonly IAccountService accounts;
    private readonly IArtifactService artifacts;
    private readonly IEmailService emails;

    public DefaultProjectService(
        IDocumentSession db,
        IUserProvider userProvider,
        IAccountService accounts,
        IArtifactService artifacts,
        IEmailService emails)
    {
        this.db = db;
        this.userProvider = userProvider;
        this.accounts = accounts;
        this.artifacts = artifacts;
        this.emails = emails;
    }

    public async Task<Hrib> Create(ProjectCreationDto dto, CancellationToken token = default)
    {
        var group = await db.LoadAsync<ProjectGroupInfo>(dto.ProjectGroupId, token);
        if (group is null)
        {
            throw new ArgumentOutOfRangeException(nameof(dto), $"Project group '{dto.ProjectGroupId}' does not exist.");
        }

        if (!userProvider.CanRead(group))
        {
            throw new UnauthorizedAccessException();
        }

        if (!group.IsOpen)
        {
            throw new ArgumentException(
                $"Project group '{dto.ProjectGroupId}' is not open for submissions.",
                nameof(dto));
        }

        var created = new ProjectCreated(
            ProjectId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            ProjectGroupId: dto.ProjectGroupId,
            Name: dto.Name,
            Visibility: Visibility.Private);
        var infoChanged = new ProjectInfoChanged(
            ProjectId: Hrib.Create(),
            Name: dto.Name,
            Description: dto.Description,
            Genre: dto.Genre);

        var authorsAdded = (await GetProjectAuthors(dto.Crew, dto.Cast, token))
            .Select(a => new ProjectAuthorAdded(
                ProjectId: created.ProjectId,
                AuthorId: a.Id,
                Kind: a.Kind,
                Roles: a.Roles));

        db.Events.StartStream<ProjectInfo>(created.ProjectId, created, infoChanged);
        db.Events.Append(created.ProjectId, authorsAdded);
        await db.SaveChangesAsync(token);

        if (userProvider.User is not null)
        {
            await accounts.AddCapabilities(
                userProvider.User.Id,
                new[] { new ProjectOwnership(created.ProjectId) },
                token);
            await userProvider.Refresh(token: token);
        }

        return created.ProjectId;
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
            foreach(var artifact in project.Artifacts)
            {
                eventStream.AppendOne(new ProjectArtifactRemoved(dto.Id, artifact.Id));
            }

            foreach(var artifact in dto.Artifacts)
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

    public async Task<ImmutableArray<ProjectListDto>> List(CancellationToken token = default)
    {
        var projects = await db.Query<ProjectInfo>()
            .WhereCanRead(userProvider)
            .ToListAsync(token);
        return projects.Select(TransferMaps.ToProjectListDto).ToImmutableArray();
    }

    public async Task<ProjectDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var data = await db.LoadAsync<ProjectInfo>(id, token);
        if (data is null)
        {
            return null;
        }

        if (!userProvider.CanRead(data))
        {
            throw new UnauthorizedAccessException();
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
