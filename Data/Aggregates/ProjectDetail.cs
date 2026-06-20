using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.Events;
using JasperFx.Events.Grouping;
using Kafe.Data.Events;
using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace Kafe.Data.Aggregates;

public record ProjectDetail(
    [property: Hrib]
    string Id,
    CreationMethod CreationMethod,
    [property: Hrib]
    string ProjectGroupId,
    [property: Hrib]
    string? ArtifactId,
    ImmutableArray<ProjectReviewInfo> Reviews,
    [property: Hrib]
    string? OwnerId,
    string? AiUsageDeclaration = null,
    string? HearAboutUs = null,
    Permission GlobalPermissions = Permission.None,
    [property: Sortable]
    bool IsLocked = false,
    [property: Sortable]
    LocalizedString? Name = null
) : IVisibleEntity
{
    public static readonly ProjectDetail Invalid = new();

    static string IKafeTypeMetadata.Moniker => "project";

    static LocalizedString IKafeTypeMetadata.Title { get; } = LocalizedString.Create(
        (Const.InvariantCulture, "Project"),
        (Const.CzechCulture, "Projekt")
    );

    Hrib IEntity.Id => Id;

    public ProjectDetail() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        ProjectGroupId: Hrib.InvalidValue,
        ArtifactId: Hrib.InvalidValue,
        Reviews: [],
        GlobalPermissions: Permission.None,
        IsLocked: false,
        OwnerId: null,
        Name: LocalizedString.Empty
    )
    {
    }
}

public class ProjectDetailProjection : MultiStreamProjection<ProjectDetail, string>
{
    public ProjectDetailProjection()
    {
        Identity<ProjectCreated>(e => e.ProjectId);
        Identity<ProjectLocked>(e => e.ProjectId);
        Identity<ProjectUnlocked>(e => e.ProjectId);
        Identity<ProjectReviewAdded>(e => e.ProjectId);
        Identity<ProjectGlobalPermissionsChanged>(e => e.ProjectId);
        CustomGrouping(new Grouper());
    }

    public static ProjectDetail Create(ProjectCreated e)
    {
        return new ProjectDetail(
            Id: e.ProjectId,
            CreationMethod: e.CreationMethod,
            ProjectGroupId: e.ProjectGroupId,
            ArtifactId: e.ArtifactId,
            Reviews: [],
            OwnerId: e.OwnerId
        );
    }

    public ProjectDetail Apply(ProjectArtifactSet e, ProjectDetail p)
    {
        return p with
        {
            ArtifactId = e.ArtifactId
        };
    }

    public ProjectDetail Apply(ProjectArtifactMetadataChanged e, ProjectDetail p)
    {
        return p with
        {
            ArtifactId = e.ArtifactId,
            Name = e.Name
        };
    }

    public ProjectDetail Apply(ProjectLocked _, ProjectDetail p)
    {
        return p with { IsLocked = true };
    }

    public ProjectDetail Apply(ProjectUnlocked _, ProjectDetail p)
    {
        return p with { IsLocked = false };
    }

    public ProjectDetail Apply(IEvent<ProjectReviewAdded> e, ProjectDetail p)
    {
        return p with
        {
            Reviews = p.Reviews.Add(
                new ProjectReviewInfo(
                    ReviewerId: e.Data.ReviewerId,
                    Kind: e.Data.Kind,
                    ReviewerRole: e.Data.ReviewerRole,
                    Comment: e.Data.Comment,
                    AddedOn: e.Timestamp
                )
            )
        };
    }

    public ProjectDetail Apply(ProjectGlobalPermissionsChanged e, ProjectDetail p)
    {
        return p with
        {
            GlobalPermissions = e.GlobalPermissions
        };
    }

    public class Grouper : IAggregateGrouper<string>
    {
        public async Task Group(IQuerySession session, IEnumerable<IEvent> events, IEventGrouping<string> grouping)
        {
            var artifactAssignments = events.OfType<IEvent<ProjectArtifactSet>>().ToImmutableArray();
            if (artifactAssignments.Length > 0)
            {
                var artifactIds = artifactAssignments.Select(e => e.Data.ArtifactId).ToImmutableHashSet();
                var artifactNames = (await session.Query<ArtifactInfo>()
                        .Where(a => artifactIds.Contains(a.Id))
                        .Select(a => new
                            {
                                Id = a.Id,
                                Name = a.Properties.GetValueOrDefault(nameof(ArtifactInfo.Name))
                            }
                        )
                        .ToListAsync())
                    .Where(p => p.Name.Value is LocalizedString)
                    .ToImmutableDictionary(p => p.Id, p => (LocalizedString)p.Name.Value);
                foreach (var artifactAssignment in artifactAssignments)
                {
                    grouping.AddEvent(
                        artifactAssignment.Data.ProjectId,
                        artifactAssignment.WithData(
                            new ProjectArtifactMetadataChanged(
                                ProjectId: artifactAssignment.Data.ProjectId,
                                ArtifactId: artifactAssignment.Data.ArtifactId,
                                Name: artifactNames.GetValueOrDefault(artifactAssignment.Data.ArtifactId)
                            )
                        )
                    );
                }
            }

            var artifactInfoChanges = events.OfType<IEvent<ArtifactInfoChanged>>()
                .Where(e => e.Data.Name is not null)
                .Select(e => (artifactId: e.Data.ArtifactId, name: (LocalizedString?)e.Data.Name, @event: (IEvent)e));
            var artifactNamePropertyChanges = events.OfType<IEvent<ArtifactPropertiesSet>>()
                .Where(e => e.Data.Properties.ContainsKey(nameof(ArtifactInfo.Name)))
                .Select(e => (
                        artifactId: e.Data.ArtifactId,
                        name: e.Data.Properties[nameof(ArtifactInfo.Name)].Object?.Value as LocalizedString,
                        @event: (IEvent)e
                    )
                );
            var newArtifactNames = artifactInfoChanges.Concat(artifactNamePropertyChanges)
                .Where(p => p.name is not null)
                .ToImmutableDictionary(p => p.artifactId, p => (name: p.name, @events: p.@event));
            if (newArtifactNames.Count > 0)
            {
                var artifactProjectMap = (await session.Query<ProjectInfo>()
                        .Where(p => p.ArtifactId != null && newArtifactNames.ContainsKey(p.ArtifactId))
                        .Select(p => new { ProjectId = p.Id, ArtifactId = p.ArtifactId! })
                        .ToListAsync())
                    .ToImmutableDictionary(p => p.ArtifactId, p => p.ProjectId);
                foreach (var (artifactId, (name, @event)) in newArtifactNames)
                {
                    if (!artifactProjectMap.TryGetValue(artifactId, out var projectId))
                    {
                        continue;
                    }

                    grouping.AddEvent(
                        projectId,
                        @event.WithData(
                            new ProjectArtifactMetadataChanged(
                                ProjectId: projectId,
                                ArtifactId: artifactId,
                                Name: name
                            )
                        )
                    );
                }
            }
        }
    }
}
