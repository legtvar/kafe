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
    private readonly IEmailService emails;

    public DefaultProjectService(
        IDocumentSession db,
        IUserProvider userProvider,
        IAccountService accounts,
        IEmailService emails)
    {
        this.db = db;
        this.userProvider = userProvider;
        this.accounts = accounts;
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
        db.Events.StartStream<ProjectInfo>(created.ProjectId, created, infoChanged);

        var authorInfos = (await db.LoadManyAsync<AuthorInfo>(
            dto.Cast.Select(a => a.Id)
                .Concat(dto.Crew.Select(a => a.Id))))
            .ToImmutableDictionary(a => a.Id);

        var authorsAdded = new List<ProjectAuthorAdded>();

        void AddAuthors(IEnumerable<ProjectCreationAuthorDto> authors, ProjectAuthorKind kind)
        {
            foreach (var author in authors)
            {
                if (!authorInfos.TryGetValue(author.Id, out var info))
                {
                    throw new ArgumentException($"Author '{author.Id}' does not exist.");
                }

                if (!userProvider.CanRead(info))
                {
                    throw new UnauthorizedAccessException($"The user is not authorized to access user '{author.Id}'.");
                }

                authorsAdded.Add(new ProjectAuthorAdded(
                    ProjectId: created.ProjectId,
                    AuthorId: author.Id,
                    Kind: ProjectAuthorKind.Cast,
                    Roles: author.Roles.IsDefaultOrEmpty
                        ? ImmutableArray<string>.Empty
                        : author.Roles));
            }
        }

        AddAuthors(dto.Cast, ProjectAuthorKind.Cast);
        AddAuthors(dto.Crew, ProjectAuthorKind.Crew);
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
}
