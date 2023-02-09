using System;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Project(
    string Id,
    CreationMethod CreationMethod,
    string ProjectGroupId,
    ImmutableArray<ProjectAuthor> Authors,
    string? PrimaryArtifactId,
    ImmutableArray<string> ArtifactIds,
    LocalizedString Name,
    LocalizedString? Description = null,
    Visibility Visibility = Visibility.Unknown,
    DateTimeOffset ReleaseDate = default,
    bool IsLocked = false
) : IEntity;

public record ProjectAuthor(
    string Id,
    ImmutableArray<string> Jobs
) : IEntity;

public class ProjectProjection : SingleStreamAggregation<Project>
{
    public ProjectProjection()
    {
    }

    public Project Create(IEvent<ProjectCreated> e)
    {
        return new Project(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ProjectGroupId: e.Data.ProjectGroupId,
            Authors: ImmutableArray<ProjectAuthor>.Empty,
            PrimaryArtifactId: null,
            ArtifactIds: ImmutableArray<string>.Empty,
            Name: e.Data.Name,
            Visibility: e.Data.Visibility);
    }

    public Project Apply(ProjectInfoChanged e, Project p)
    {
        return p with
        {
            Name = e.Name ?? p.Name,
            Description = e.Description ?? p.Description,
            Visibility = e.Visibility ?? p.Visibility,
            ReleaseDate = e.ReleaseDate.HasValue ? e.ReleaseDate.Value : p.ReleaseDate,
            PrimaryArtifactId = e.PrimaryArtifactId ?? p.PrimaryArtifactId
        };
    }

    public Project Apply(ProjectAuthorAdded e, Project p)
    {
        if (p.Authors.IsDefault)
        {
            p = p with { Authors = ImmutableArray<ProjectAuthor>.Empty };
        }

        var author = p.Authors.SingleOrDefault(a => a.Id == e.AuthorId);
        if (author is not null && e.Jobs.HasValue && !e.Jobs.Value.IsDefault)
        {
            author.Jobs
                .Union(e.Jobs)
                .ToImmutableArray();
        }
        else
        {
            author = new ProjectAuthor(e.AuthorId, e.Jobs.HasValue && !e.Jobs.Value.IsDefault
                ? e.Jobs.Value : ImmutableArray.Create<string>());
        }

        return p with
        {
            Authors = p.Authors.RemoveAll(a => a.Id == e.AuthorId)
                .Add(author)
        };
    }

    public Project Apply(ProjectAuthorRemoved e, Project p)
    {
        if (p.Authors.IsDefault)
        {
            return p;
        }

        return p with
        {
            Authors = p.Authors.RemoveAll(a => a.Id == e.AuthorId)
        };
    }

    public Project Apply(ProjectArtifactAdded e, Project p)
    {
        if (p.ArtifactIds.IsDefault)
        {
            p = p with { ArtifactIds = ImmutableArray<string>.Empty };
        }

        return p with
        {
            ArtifactIds = p.ArtifactIds.RemoveAll(a => a == e.ArtifactId)
                .Add(e.ArtifactId)
        };
    }

    public Project Apply(ProjectArtifactRemoved e, Project p)
    {
        if (p.ArtifactIds.IsDefault)
        {
            return p;
        }

        return p with
        {
            ArtifactIds = p.ArtifactIds.RemoveAll(a => a == e.ArtifactId),
            PrimaryArtifactId = p.PrimaryArtifactId == e.ArtifactId ? null : p.PrimaryArtifactId
        };
    }

    public Project Apply(ProjectLocked _, Project p)
    {
        return p with { IsLocked = true };
    }

    public Project Apply(ProjectUnlocked _, Project p)
    {
        return p with { IsLocked = false };
    }
}
