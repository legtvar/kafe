using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record Artifact(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    string ProjectId,
    ImmutableArray<string> ShardIds) : IEntity;

public class ArtifactProjection : SingleStreamAggregation<Artifact>
{
    public ArtifactProjection()
    {
    }

    public Artifact Create(IEvent<ArtifactCreated> e)
    {
        return new Artifact(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            ProjectId: e.Data.ProjectId,
            ShardIds: ImmutableArray<string>.Empty);
    }

    public Artifact Apply(ArtifactInfoChanged e, Artifact a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            ProjectId = e.ProjectId ?? a.ProjectId
        };
    }

    public Artifact Apply(ArtifactShardAdded e, Artifact a)
    {
        if (a.ShardIds.Contains(e.ShardId))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Add(e.ShardId)
        };
    }

    public Artifact Apply(ArtifactShardRemoved e, Artifact a)
    {
        if (!a.ShardIds.Contains(e.ShardId))
        {
            return a;
        }

        return a with
        {
            ShardIds = a.ShardIds.Remove(e.ShardId)
        };
    }
}
