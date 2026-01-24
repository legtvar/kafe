using Kafe.Media.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Media;

public sealed class MediaMod : IMod
{
    public static string Moniker => "media";

    public void Configure(ModContext context)
    {
        ConfigureServices(context.Services);

        context.AddShardPayload<MediaInfo>(new ShardPayloadRegistrationOptions()
        {
            AnalyzerTypes = [typeof(MediaShardAnalyzer)]
        });
        context.AddShardPayload<ImageInfo>(new ShardPayloadRegistrationOptions()
        {
            AnalyzerTypes = [typeof(ImageShardAnalyzer)]
        });
        context.AddShardPayload<SubtitlesInfo>(new ShardPayloadRegistrationOptions()
        {
            AnalyzerTypes = [typeof(SubtitlesShardAnalyzer)]
        });
        context.AddShardLinkPayload<VariantShardLink>();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMediaService, FFmpegCoreService>();
        services.AddSingleton<IImageService, ImageSharpService>();
        services.AddSingleton<MediaShardAnalyzer>();
        services.AddSingleton<SubtitlesShardAnalyzer>();
        services.AddSingleton<ImageShardAnalyzer>();
        services.AddHostedService<VideoConversionDaemon>();
    }
}
