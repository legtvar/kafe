using System;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record ProjectInfo(
    string Id,
    CreationMethod CreationMethod,
    string ProjectGroupId,
    ImmutableArray<ProjectAuthor> Authors,
    ImmutableArray<ProjectArtifact> Artifacts,
    LocalizedString Name,
    LocalizedString? Description = null,
    LocalizedString? Genre = null,
    Visibility Visibility = Visibility.Unknown,
    DateTimeOffset ReleasedOn = default,
    bool IsLocked = false
) : IEntity;

public record ProjectAuthor(
    string Id,
    ProjectAuthorKind Kind,
    ImmutableArray<string> Roles
) : IEntity;

public record ProjectArtifact(
    string Id,
    string? BlueprintSlot
) : IEntity;

public class ProjectInfoProjection : SingleStreamAggregation<ProjectInfo>
{
    public ProjectInfoProjection()
    {
    }

    public ProjectInfo Create(ProjectCreated e)
    {
        return new ProjectInfo(
            Id: e.ProjectId,
            CreationMethod: e.CreationMethod,
            ProjectGroupId: e.ProjectGroupId,
            Authors: ImmutableArray<ProjectAuthor>.Empty,
            Artifacts: ImmutableArray<ProjectArtifact>.Empty,
            Name: e.Name,
            Visibility: e.Visibility);
    }

    public ProjectInfo Apply(ProjectInfoChanged e, ProjectInfo p)
    {
        return p with
        {
            Name = e.Name ?? p.Name,
            Description = e.Description ?? p.Description,
            Visibility = e.Visibility ?? p.Visibility,
            ReleasedOn = e.ReleasedOn ?? p.ReleasedOn,
            Genre = e.Genre ?? p.Genre
        };
    }

    public ProjectInfo Apply(ProjectAuthorAdded e, ProjectInfo p)
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

    public ProjectInfo Apply(ProjectAuthorRemoved e, ProjectInfo p)
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

    public ProjectInfo Apply(ProjectArtifactAdded e, ProjectInfo p)
    {
        var projectArtifact = new ProjectArtifact(
            Id: e.ArtifactId,
            BlueprintSlot: e.BlueprintSlot);

        if (p.Artifacts.IsDefault)
        {
            p = p with
            {
                Artifacts = ImmutableArray.Create(projectArtifact)
            };
        }

        return p with
        {
            Artifacts = p.Artifacts.RemoveAll(a => a.Id == e.ArtifactId)
                .Add(projectArtifact)
        };
    }

    public ProjectInfo Apply(ProjectArtifactRemoved e, ProjectInfo p)
    {
        if (p.Artifacts.IsDefault)
        {
            return p;
        }

        return p with
        {
            Artifacts = p.Artifacts.RemoveAll(a => a.Id == e.ArtifactId)
        };
    }

    public ProjectInfo Apply(ProjectLocked _, ProjectInfo p)
    {
        return p with { IsLocked = true };
    }

    public ProjectInfo Apply(ProjectUnlocked _, ProjectInfo p)
    {
        return p with { IsLocked = false };
    }
}
