using System;
using System.Collections.Immutable;
using System.Linq;
using JasperFx.Events;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Aggregates;

public record ProjectInfo(
    [property:Hrib]
    string Id,

    CreationMethod CreationMethod,

    [property:Hrib]
    string ProjectGroupId,

    [property:Hrib]
    string ArtifactId,

    ImmutableArray<ProjectAuthorInfo> Authors,

    ImmutableArray<ProjectArtifactInfo> Artifacts,

    ImmutableArray<ProjectReviewInfo> Reviews,

    [property:Sortable]
    [property:LocalizedString]
    ImmutableDictionary<string, string> Name,

    [Hrib] string? OwnerId,

    [property:Sortable]
    [property:LocalizedString]
    ImmutableDictionary<string, string>? Description = null,

    [property:Sortable]
    [property:LocalizedString]
    ImmutableDictionary<string, string>? Genre = null,

    Permission GlobalPermissions = Permission.None,

    [property:Sortable]
    DateTimeOffset ReleasedOn = default,

    [property:Sortable]
    bool IsLocked = false
) : IVisibleEntity
{
    public static readonly ProjectInfo Invalid = new();

    static string IKafeTypeMetadata.Moniker => "project";

    static LocalizedString IKafeTypeMetadata.Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Project"),
        (Const.CzechCulture, "Projekt")
    );

    Hrib IEntity.Id => Id;

    public ProjectInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        ProjectGroupId: Hrib.InvalidValue,
        ArtifactId: Hrib.InvalidValue,
        Authors: [],
        Artifacts: [],
        Reviews: [],
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        Description: null,
        Genre: null,
        GlobalPermissions: Permission.None,
        ReleasedOn: default,
        IsLocked: false,
        OwnerId: null
    )
    {
    }

    /// <summary>
    /// Creates a bare-bones but valid <see cref="ProjectInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static ProjectInfo Create(Hrib projectGroupId, LocalizedString name)
    {
        return new()
        {
            Id = Hrib.EmptyValue,
            ProjectGroupId = projectGroupId.RawValue,
            Name = name
        };
    }
}

public record ProjectAuthorInfo(
    [Hrib] string Id,
    ProjectAuthorKind Kind,
    ImmutableArray<string> Roles
);

public record ProjectArtifactInfo(
    [Hrib] string Id,
    string? BlueprintSlot
);

public record ProjectReviewInfo(
    [Hrib] string? ReviewerId,
    ReviewKind Kind,
    string ReviewerRole,
    [LocalizedString] ImmutableDictionary<string, string>? Comment,
    DateTimeOffset AddedOn
);

public class ProjectInfoProjection : SingleStreamProjection<ProjectInfo, string>
{
    public ProjectInfoProjection()
    {
    }

    public static ProjectInfo Create(ProjectCreated e)
    {
        return new ProjectInfo(
            Id: e.ProjectId,
            CreationMethod: e.CreationMethod,
            ProjectGroupId: e.ProjectGroupId,
            ArtifactId:
            Authors: ImmutableArray<ProjectAuthorInfo>.Empty,
            Artifacts: ImmutableArray<ProjectArtifactInfo>.Empty,
            Reviews: ImmutableArray<ProjectReviewInfo>.Empty,
            Name: e.Name,
            OwnerId: e.OwnerId
        );
    }

    public ProjectInfo Apply(ProjectInfoChanged e, ProjectInfo p)
    {
        return p with
        {
            Name = e.Name ?? p.Name,
            Description = e.Description ?? p.Description,
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
                Roles: e.Roles.HasValue && !e.Roles.Value.IsDefault ? e.Roles.Value : []);
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
                Artifacts = [projectArtifact]
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
                ReviewerId: e.Data.ReviewerId,
                Kind: e.Data.Kind,
                ReviewerRole: e.Data.ReviewerRole,
                Comment: e.Data.Comment,
                AddedOn: e.Timestamp)
            )
        };
    }

    public ProjectInfo Apply(ProjectGlobalPermissionsChanged e, ProjectInfo a)
    {
        return a with
        {
            GlobalPermissions = e.GlobalPermissions
        };
    }
}
