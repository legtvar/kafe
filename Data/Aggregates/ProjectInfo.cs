using System;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record ProjectInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [Hrib] string ProjectGroupId,
    ImmutableArray<ProjectAuthorInfo> Authors,
    ImmutableArray<ProjectArtifactInfo> Artifacts,
    ImmutableArray<ProjectReviewInfo> Reviews,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    [LocalizedString] ImmutableDictionary<string, string>? Genre = null,
    Visibility Visibility = Visibility.Unknown,
    DateTimeOffset ReleasedOn = default,
    bool IsLocked = false
) : IVisibleEntity, IHierarchicalEntity
{
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string ParentId => ProjectGroupId;
}

public record ProjectAuthorInfo(
    [Hrib] string Id,
    ProjectAuthorKind Kind,
    ImmutableArray<string> Roles
) : IEntity;

public record ProjectArtifactInfo(
    [Hrib] string Id,
    string? BlueprintSlot
) : IEntity;

public record ProjectReviewInfo(
    ReviewKind Kind,
    string ReviewerRole,
    [LocalizedString] ImmutableDictionary<string, string>? Comment,
    DateTimeOffset AddedOn
);

public class ProjectInfoProjection : SingleStreamProjection<ProjectInfo>
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
            Authors: ImmutableArray<ProjectAuthorInfo>.Empty,
            Artifacts: ImmutableArray<ProjectArtifactInfo>.Empty,
            Reviews: ImmutableArray<ProjectReviewInfo>.Empty,
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
            p = p with { Authors = ImmutableArray<ProjectAuthorInfo>.Empty };
        }

        var author = p.Authors.SingleOrDefault(a => a.Id == e.AuthorId && a.Kind == e.Kind);
        if (author is not null && e.Roles.HasValue && !e.Roles.Value.IsDefault)
        {
            author.Roles
                .Union(e.Roles)
                .ToImmutableArray();
        }
        else
        {
            author = new ProjectAuthorInfo(
                Id: e.AuthorId,
                Kind: e.Kind,
                Roles: e.Roles.HasValue && !e.Roles.Value.IsDefault
                    ? e.Roles.Value
                    : ImmutableArray.Create<string>());
        }

        return p with
        {
            Authors = p.Authors.Add(author)
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
                Authors = p.Authors.RemoveAll(a => a.Id == e.AuthorId && a.Kind == (e.Kind ?? a.Kind))
            };
        }

        return p with
        {
            Authors = p.Authors
                .Select(a =>
                {
                    if (a.Id != e.AuthorId || a.Kind != (e.Kind ?? a.Kind))
                    {
                        return a;
                    }

                    return a with
                    {
                        Roles = a.Roles.RemoveAll(r => e.Roles.Value.Contains(r))
                    };
                })
                .Where(a => a.Roles.Length > 0)
                .ToImmutableArray()
        };

    }

    public ProjectInfo Apply(ProjectArtifactAdded e, ProjectInfo p)
    {
        var projectArtifact = new ProjectArtifactInfo(
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

    public ProjectInfo Apply(IEvent<ProjectReviewAdded> e, ProjectInfo p)
    {
        return p with
        {
            Reviews = p.Reviews.Add(new ProjectReviewInfo(
                Kind: e.Data.Kind,
                ReviewerRole: e.Data.ReviewerRole,
                Comment: e.Data.Comment,
                AddedOn: e.Timestamp)
            )
        };
    }
}
