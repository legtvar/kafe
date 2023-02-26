using Kafe.Data.Events;
using Kafe.Media;
using Marten;
using Marten.Events.Projections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Aggregates;

public record ArtifactDetail(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    ImmutableArray<ShardInfoBase> Shards
);

public class ArtifactDetailProjection : EventProjection
{
    public ArtifactDetail Create(ArtifactCreated e)
    {
        return new ArtifactDetail(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            Shards: ImmutableArray<ShardInfoBase>.Empty);
    }

    public async Task Project(IShardEvent e, IDocumentOperations ops)
    {
        ShardInfo? shard = e switch
        {
            IVideoShardEvent => await ops.Events.AggregateStreamAsync<VideoShardInfo>(e.ShardId),
            IImageShardEvent => await ops.Events.AggregateStreamAsync<ImageShardInfo>(e.ShardId),
            ISubtitlesShardEvent => await ops.Events.AggregateStreamAsync<SubtitlesShardInfo>(e.ShardId),
            _ => throw new NotImplementedException($"{nameof(ArtifactDetail)} does not support {e.GetType()}.")
        };

        if (shard is null)
        {
            throw new ArgumentException($"Aggregation of the '{e.ShardId}' shard failed.");
        }

        var artifact = await ops.LoadAsync<ArtifactDetail>(shard.ArtifactId);
        if (artifact is null)
        {
            throw new InvalidOperationException($"The '{e}' event references an artifact that does not exist.");
        }

        artifact = artifact with
        {
            Shards = artifact.Shards
                .RemoveAll(s => s.Id == shard.Id)
                .Add(shard)
        };
        ops.Update(artifact);
    }
}
