using Kafe.Data.Aggregates;
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

namespace Kafe.Data.Documents;

public record ArtifactDetail(
    Hrib Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    ImmutableArray<Shard> Shards);

public class ArtifactDetailProjection : EventProjection
{
    public ArtifactDetail Create(ArtifactCreated e)
    {
        return new ArtifactDetail(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            Shards: ImmutableArray<Shard>.Empty);
    }

    public async Task Project(IShardCreated e, IDocumentOperations ops)
    {
        Shard shard = e switch
        {
            VideoShardCreated => new VideoShard(
                Id: e.ShardId,
                CreationMethod: e.CreationMethod,
                ArtifactId: e.ArtifactId,
                Variants: ImmutableArray<VideoQualityPreset>.Empty),
            ImageShardCreated => new ImageShard(
                Id: e.ShardId,
                CreationMethod: e.CreationMethod,
                ArtifactId: e.ArtifactId,
                Variants: ImmutableArray<ImageQualityPreset>.Empty),
            SubtitlesShardCreated => new SubtitlesShard(
                Id: e.ShardId,
                CreationMethod: e.CreationMethod,
                ArtifactId: e.ArtifactId,
                Variants: ImmutableArray<string>.Empty),
            _ => throw new NotImplementedException($"{nameof(ArtifactDetail)} does not support {e.GetType()}.")
        };

        var artifact = await ops.LoadAsync<ArtifactDetail>(e.ArtifactId);
        if (artifact is null)
        {
            throw new InvalidOperationException($"The {e} event references an artifact that does not exist.");
        }

        artifact = artifact with
        {
            Shards = artifact.Shards.Add(shard)
        };
        ops.Update(artifact);
    }
}
