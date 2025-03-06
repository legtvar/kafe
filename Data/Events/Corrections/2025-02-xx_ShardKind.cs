using System;
using Kafe.Media;

namespace Kafe.Data
{
    [Obsolete("Use the new shard universal shard events instead.")]
    public enum ShardKind
    {
        Unknown = 0,
        Video = 1,
        Image = 2,
        Subtitles = 3,
        Blend = 4
    }
}

namespace Kafe.Data.Events
{
    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardEvent
    {
        [Hrib]
        string ShardId { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardCreated : IShardEvent
    {
        CreationMethod CreationMethod { get; }

        [Hrib]
        string ArtifactId { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardVariantAdded : IShardEvent
    {
        string Name { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardVariantRemoved : IShardEvent
    {
        string Name { get; }
    }
    
    [Obsolete("Use the new shard universal shard events instead.")]
    public interface ISubtitlesShardEvent : IShardEvent
    {
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public record SubtitlesShardCreated(
        [Hrib] string ShardId,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        SubtitlesInfo OriginalVariantInfo
    ) : ISubtitlesShardEvent, IShardCreated;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record SubtitlesShardVariantsAdded(
        [Hrib] string ShardId,
        string Name,
        SubtitlesInfo Info
    ) : ISubtitlesShardEvent, IShardVariantAdded;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record SubtitlesShardVariantsRemoved(
        [Hrib] string ShardId,
        string Name
    ) : ISubtitlesShardEvent, IShardVariantRemoved;

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IVideoShardEvent : IShardEvent
    {
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public record VideoShardCreated(
        [Hrib] string ShardId,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        MediaInfo OriginalVariantInfo
    ) : IVideoShardEvent, IShardCreated;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record VideoShardVariantAdded(
        [Hrib] string ShardId,
        string Name,
        MediaInfo Info
    ) : IVideoShardEvent, IShardVariantAdded;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record VideoShardVariantRemoved(
        [Hrib] string ShardId,
        string Name
    ) : IVideoShardEvent, IShardVariantRemoved;

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IImageShardEvent : IShardEvent
    {
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public record ImageShardCreated(
        [Hrib] string ShardId,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        ImageInfo OriginalVariantInfo
    ) : IImageShardEvent, IShardCreated;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record ImageShardVariantsAdded(
        [Hrib] string ShardId,
        string Name,
        ImageInfo Info
    ) : IImageShardEvent, IShardVariantAdded;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record ImageShardVariantsRemoved(
        [Hrib] string ShardId,
        string Name
    ) : IImageShardEvent, IShardVariantRemoved;
}
