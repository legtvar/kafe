namespace Kafe.Media;

[Mod(Name)]
public sealed class MediaMod : IMod
{
    public const string Name = "media";

    public KafeType AudiovisualShardType { get; private set; }

    public KafeType ImageShardType { get; private set; }

    public KafeType SubtitlesShardType { get; private set; }

    public void Configure(ModContext context)
    {
        AudiovisualShardType = context.AddShard<MediaInfo>(new()
        {
            Name = "audiovisual",
        });
        ImageShardType = context.AddShard<ImageInfo>(new()
        {
            Name = "image"
        });
        SubtitlesShardType = context.AddShard<SubtitlesInfo>(new()
        {
            Name = "subtitles"
        });
    }
}
