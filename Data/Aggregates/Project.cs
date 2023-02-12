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
    ImmutableArray<string> ArtifactIds,
    LocalizedString Name,
    LocalizedString? Description = null,
    LocalizedString? Genre = null,
    Visibility Visibility = Visibility.Unknown,
    DateTimeOffset ReleaseDate = default,
    bool IsLocked = false
) : IEntity;

public record ProjectAuthor(
    string Id,
    ProjectAuthorKind Kind,
    ImmutableArray<string> Roles
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
            ReleaseDate = e.ReleaseDate ?? p.ReleaseDate,
            Genre = e.Genre ?? p.Genre
        };
    }

    public Project Apply(ProjectAuthorAdded e, Project p)
    {
        if (p.Authors.IsDefault)
        {
            p = p with { Authors = ImmutableArray<ProjectAuthor>.Empty };
        }

        var author = p.Authors.SingleOrDefault(a => a.Id == e.AuthorId);
        if (author is not null && e.Roles.HasValue && !e.Roles.Value.IsDefault)
        {
            author.Roles
                .Union(e.Roles)
                .ToImmutableArray();
        }
        else
        {
            author = new ProjectAuthor(
                Id: e.AuthorId,
                Kind: e.Kind,
                Roles: e.Roles.HasValue && !e.Roles.Value.IsDefault
                    ? e.Roles.Value
                    : ImmutableArray.Create<string>());
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

        if (e.Roles is null)
        {
            return p with
            {
                Authors = p.Authors.RemoveAll(a => a.Id == e.AuthorId)
            };
        }
        else
        {
            return p with
            {
                Authors = p.Authors
                    .Select(a =>
                    {
                        if (a.Id != e.AuthorId)
                        {
                            return a;
                        }

                        return a with
                        {
                            Roles = a.Roles.RemoveAll(r => e.Roles.Value.Contains(r))
                                .ToImmutableArray()
                        };
                    })
                    .ToImmutableArray()
            };
        }

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
            ArtifactIds = p.ArtifactIds.RemoveAll(a => a == e.ArtifactId)
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
