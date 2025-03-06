#pragma warning disable 0618

using System.Collections.Generic;
using System.Collections.Immutable;
using Kafe.Media;
using Marten.Services.Json.Transformations;

namespace Kafe.Data.Events.Upcasts
{
    internal class VideoShardCreatedUpcaster : EventUpcaster<VideoShardCreated, ShardCreated>
    {
        protected override ShardCreated Upcast(VideoShardCreated oldEvent)
        {
            return new ShardCreated(
                ShardId: oldEvent.ShardId,
                CreationMethod: oldEvent.CreationMethod,
                ArtifactId: oldEvent.ArtifactId,
                Size: oldEvent.OriginalVariantInfo.FileLength,
                Filename: null!,
                Metadata: new(new("media", "shard", "video", false), new VideoShard
                {
                    Variants = ImmutableDictionary.CreateRange(
                        [
                            new KeyValuePair<string, MediaInfo>(
                                Const.OriginalShardVariant,
                                oldEvent.OriginalVariantInfo
                            )
                        ]
                    )
                })
            );
        }
    }
}
