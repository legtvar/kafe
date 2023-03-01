﻿using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultProjectService : IProjectService
{
    private readonly IDocumentSession db;

    public DefaultProjectService(IDocumentSession db)
    {
        this.db = db;
    }

    public async Task<Hrib> Create(ProjectCreationDto dto, CancellationToken token = default)
    {
        var group = await db.LoadAsync<ProjectGroupInfo>(dto.ProjectGroupId);
        if (group is null)
        {
            throw new ArgumentException($"Project group '{dto.ProjectGroupId}' does not exist.");
        }

        var created = new ProjectCreated(
            ProjectId: Hrib.Create(),
            CreationMethod: CreationMethod.Api,
            ProjectGroupId: dto.ProjectGroupId,
            Name: dto.Name,
            Visibility: Visibility.Private);
        db.Events.StartStream<ProjectInfo>(created.ProjectId, created);

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

                authorsAdded.Add(new ProjectAuthorAdded(
                    ProjectId: created.ProjectId,
                    AuthorId: author.Id,
                    Kind: ProjectAuthorKind.Cast,
                    Roles: author.Roles));
            }
        }

        AddAuthors(dto.Cast, ProjectAuthorKind.Cast);
        AddAuthors(dto.Crew, ProjectAuthorKind.Crew);

        await db.SaveChangesAsync(token);
        return created.ProjectId;
    }

    public async Task<ImmutableArray<ProjectListDto>> List(CancellationToken token = default)
    {
        var projects = await db.Query<ProjectInfo>().ToListAsync(token);
        return projects.Select(TransferMaps.ToProjectListDto).ToImmutableArray();
    }

    public async Task<ProjectDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var data = await db.LoadAsync<ProjectInfo>(id, token);
        if (data is null)
        {
            return null;
        }

        var group = await db.LoadAsync<ProjectGroupInfo>(data.ProjectGroupId, token);
        var artifactDetails = await db.LoadManyAsync<ArtifactDetail>(token, data.ArtifactIds);
        var authors = await db.LoadManyAsync<AuthorInfo>(token, data.Authors.Select(a => a.Id));
        var dto = TransferMaps.ToProjectDetailDto(data) with
        {
            ProjectGroupName = group?.Name ?? Const.UnknownProjectGroup,
            Artifacts = artifactDetails.Select(TransferMaps.ToArtifactDetailDto).ToImmutableArray(),
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
