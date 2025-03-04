namespace Kafe.Media;

[Mod(Name)]
public sealed class MediaMod : IMod
{
    public const string Name = "media";

    public void Configure(ModContext context)
    {
        context.AddShard<VideoShard>(new()
        {
            Name = "video",
        });
        context.AddShard<ImageShard>(new()
        {
            Name = "image"
        });
        context.AddShard<SubtitlesShard>(new()
        {
            Name = "subtitles"
        });
    }
}
