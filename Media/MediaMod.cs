namespace Kafe.Media;

[Mod(Name)]
public sealed class MediaMod : IMod
{
    public const string Name = "media";

    public KafeType VideoShardType { get; private set; }

    public KafeType ImageShardType { get; private set; }

    public KafeType SubtitlesShardType { get; private set; }

    public void Configure(ModContext context)
    {
        VideoShardType = context.AddShard<VideoShard>(new()
        {
            Name = "video",
        });
        ImageShardType = context.AddShard<ImageShard>(new()
        {
            Name = "image"
        });
        SubtitlesShardType = context.AddShard<SubtitlesShard>(new()
        {
            Name = "subtitles"
        });
    }
}
