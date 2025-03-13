#pragma warning disable 0618

using System;
using System.Collections.Immutable;
using Kafe.Media;

namespace Kafe.Data
{
    [Obsolete("Use the new universal shard events instead.")]
    public enum ShardKind
    {
        Unknown = 0,
        Video = 1,
        Image = 2,
        Subtitles = 3,
        Blend = 4
    }
}

namespace Kafe.Data.Aggregates
{
    [Obsolete("Use the new universal ShardInfo projection instead.")]
    public abstract record ShardInfoBase(
        [Hrib] string Id,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        DateTimeOffset CreatedAt) : IShardEntity
    {
        public abstract ShardKind Kind { get; }
    }

    [Obsolete("Use the new universal ShardInfo projection instead.")]
    public record VideoShardInfo(
        [Hrib] string Id,
        CreationMethod CreationMethod,
        DateTimeOffset CreatedAt,
        [Hrib] string ArtifactId,
        ImmutableDictionary<string, MediaInfo> Variants
    ) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
    {
        public override ShardKind Kind => ShardKind.Video;
    }

    [Obsolete("Use the new universal ShardInfo projection instead.")]
    public record ImageShardInfo(
        [Hrib] string Id,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        DateTimeOffset CreatedAt,
        ImmutableDictionary<string, ImageInfo> Variants
    ) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
    {
        public override ShardKind Kind => ShardKind.Image;
    }

    public record SubtitlesShardInfo(
        [Hrib] string Id,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        DateTimeOffset CreatedAt,
        ImmutableDictionary<string, SubtitlesInfo> Variants
    ) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
    {
        public override ShardKind Kind => ShardKind.Subtitles;
    }

    public record BlendShardInfo(
        [Hrib] string Id,
        CreationMethod CreationMethod,
        DateTimeOffset CreatedAt,
        [Hrib] string ArtifactId,
        ImmutableDictionary<string, BlendInfo> Variants
    ) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
    {
        public override ShardKind Kind => ShardKind.Blend;
    }
}
