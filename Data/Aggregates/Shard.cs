using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Kafe.Data.Aggregates;

public abstract record Shard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId) : IEntity
{
    public abstract ShardKind Kind { get; }
}

public record VideoShard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId,
    ImmutableArray<VideoQualityPreset> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Video;
}

public record ImageShard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId,
    ImmutableArray<ImageQualityPreset> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Image;
}

public record SubtitlesShard(
    string Id,
    CreationMethod CreationMethod,
    string ArtifactId,
    ImmutableArray<string> Variants) : Shard(Id, CreationMethod, ArtifactId)
{
    public override ShardKind Kind => ShardKind.Subtitles;
}


public class ShardProjection : SingleStreamAggregation<Shard>
{
    public ShardProjection()
    {
    }

    public VideoShard Create(IEvent<VideoShardCreated> e)
    {
        return new(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            Variants: ImmutableArray.Create(VideoQualityPreset.Original));
    }

    public VideoShard Apply(VideoShardVariantsAdded e, VideoShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }

    public ImageShard Create(IEvent<ImageShardCreated> e)
    {
        return new(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            Variants: ImmutableArray.Create(ImageQualityPreset.Original));
    }

    public ImageShard Apply(ImageShardVariantsAdded e, ImageShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }

    public SubtitlesShard Create(IEvent<SubtitlesShardCreated> e)
    {
        return new(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            ArtifactId: e.Data.ArtifactId,
            Variants: ImmutableArray.Create(CultureInfo.InvariantCulture.TwoLetterISOLanguageName));
    }

    public SubtitlesShard Apply(SubtitlesShardVariantsAdded e, SubtitlesShard s)
    {
        return s with
        {
            Variants = s.Variants.Union(e.Variants).ToImmutableArray()
        };
    }
}