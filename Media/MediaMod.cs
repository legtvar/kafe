using Kafe.Media.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Media;

public sealed class MediaMod : IMod
{
    public static string Moniker { get; } = "media";

    public KafeType AudiovisualShardType { get; private set; }

    public KafeType ImageShardType { get; private set; }

    public KafeType SubtitlesShardType { get; private set; }

    public void Configure(ModContext context)
    {
        ConfigureServices(context.Services);

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

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMediaService, FFmpegCoreService>();
        services.AddSingleton<IImageService, ImageSharpService>();
        services.AddSingleton<VideoShardAnalyzer>();
        services.AddSingleton<SubtitlesShardAnalyzer>();
        services.AddSingleton<ImageShardAnalyzer>();
        services.AddHostedService<VideoConversionDaemon>();
    }
}
