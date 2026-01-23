using System;
using System.Collections.Immutable;
using JasperFx.Events;
using Kafe.Data.Events;
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
    string? ArtifactId,

    ImmutableArray<ProjectReviewInfo> Reviews,

    [property:Hrib]
    string? OwnerId,

    Permission GlobalPermissions = Permission.None,

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
        Reviews: [],
        GlobalPermissions: Permission.None,
        IsLocked: false,
        OwnerId: null
    )
    {
    }

    /// <summary>
    /// Creates a bare-bones but valid <see cref="ProjectInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static ProjectInfo Create(Hrib projectGroupId)
    {
        return new()
        {
            Id = Hrib.EmptyValue,
            ProjectGroupId = projectGroupId.RawValue
        };
    }
}

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
            ArtifactId: e.ArtifactId,
            Reviews: [],
            OwnerId: e.OwnerId
        );
    }

    public ProjectInfo Apply(ProjectArtifactSet e, ProjectInfo p)
    {
        return p with
        {
            ArtifactId = e.ArtifactId
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
