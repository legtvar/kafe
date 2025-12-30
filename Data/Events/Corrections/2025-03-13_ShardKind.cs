#pragma warning disable 0618

using System;
using System.Collections.Immutable;
using Kafe.Media.Deprecated;

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

namespace Kafe.Media.Deprecated
{
    [Obsolete("Use `Kafe.Mate.BlendInfo` instead.")]
    public record BlendInfo(
        string FileExtension,
        string MimeType,
        string? Error = null
    );

    [Obsolete("Use `Kafe.Media.VideoStreamInfo` instead.")]
    public record VideoStreamInfo(
        string Codec,
        long Bitrate,
        int Width,
        int Height,
        double Framerate);

    [Obsolete("Use `Kafe.Media.AudioStreamInfo` instead.")]
    public record AudioStreamInfo(
        string Codec,
        long Bitrate,
        int Channels,
        int SampleRate);

    [Obsolete("Use `Kafe.Media.SubtitleStreamInfo` instead.")]
    public record SubtitleStreamInfo(
        string? Language,
        string Codec,
        long Bitrate);

    [Obsolete("Use `Kafe.Media.MediaInfo` instead.")]
    public record MediaInfo(
        string FileExtension,
        string FormatName,
        string MimeType,
        long FileLength,
        TimeSpan Duration,
        double Bitrate,
        ImmutableArray<VideoStreamInfo> VideoStreams,
        ImmutableArray<AudioStreamInfo> AudioStreams,
        ImmutableArray<SubtitleStreamInfo> SubtitleStreams,
        bool IsCorrupted = false,
        string? Error = null
    );

    [Obsolete("Use `Kafe.Media.ImageInfo` instead.")]
    public record ImageInfo(
        string FileExtension,
        string MimeType,
        string FormatName,
        int Width,
        int Height,
        bool IsCorrupted = false
    );

    [Obsolete("Use `Kafe.Media.SubtitlesInfo` instead.")]
    public record SubtitlesInfo(
        string FileExtension,
        string MimeType,
        string? Language,
        string Codec,
        long Bitrate,
        bool IsCorrupted);
}

namespace Kafe.Data.Aggregates
{
    [Obsolete("Use the new universal ShardInfo projection instead.")]
    public interface IShardEntity : IEntity
    {
        ShardKind Kind { get; }

        [Hrib]
        string ArtifactId { get; }
    }

    [Obsolete("Use the new universal ShardInfo projection instead.")]
    public abstract record ShardInfoBase(
        [Hrib] string Id,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        DateTimeOffset CreatedAt) : IShardEntity
    {
        public abstract ShardKind Kind { get; }

        Hrib IEntity.Id => Id;
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

    [Obsolete("Use the new universal ShardInfo projection instead.")]
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

    [Obsolete("Use the new universal ShardInfo projection instead.")]
    public record BlendShardInfo(
        [Hrib] string Id,
        string? FileName,
        CreationMethod CreationMethod,
        DateTimeOffset CreatedAt,
        [Hrib] string ArtifactId,
        ImmutableDictionary<string, BlendInfo> Variants
    ) : ShardInfoBase(Id, CreationMethod, ArtifactId, CreatedAt)
    {
        public override ShardKind Kind => ShardKind.Blend;
    }
}
