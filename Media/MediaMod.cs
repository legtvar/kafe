using Kafe.Media.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Media;

public sealed class MediaMod : IMod
{
    public static string Name { get; } = "media";

    public KafeType AudiovisualShardType { get; private set; }

    public KafeType ImageShardType { get; private set; }

    public KafeType SubtitlesShardType { get; private set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMediaService, FFmpegCoreService>();
        services.AddSingleton<IImageService, ImageSharpService>();
        services.AddSingleton<VideoShardAnalyzer>();
        services.AddSingleton<SubtitlesShardAnalyzer>();
        services.AddSingleton<ImageShardAnalyzer>();
    }

    public void Configure(ModContext context)
    {
        AudiovisualShardType = context.AddShard<MediaInfo>(new()
        {
            Name = "video",
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
