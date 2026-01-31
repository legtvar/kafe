using Kafe.Media.Diagnostics;
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
        context.AddShardLinkPayload<GeneratedFromShardLink>();

        context.AddDiagnosticPayload<AudioBitrateTooHighDiagnostic>();
        context.AddDiagnosticPayload<AudioBitrateTooLowDiagnostic>();
        context.AddDiagnosticPayload<AudioCodecNotAllowedDiagnostic>();
        context.AddDiagnosticPayload<MediaBitrateTooHighDiagnostic>();
        context.AddDiagnosticPayload<MediaBitrateTooLowDiagnostic>();
        context.AddDiagnosticPayload<MediaConversionAlreadyCompleted>();
        context.AddDiagnosticPayload<MediaConversionAlreadyFailed>();
        context.AddDiagnosticPayload<MediaConversionBadShardTypeDiagnostic>();
        context.AddDiagnosticPayload<MediaConversionFailedDiagnostic>();
        context.AddDiagnosticPayload<MediaShorterSideTooShortDiagnostic>();
        context.AddDiagnosticPayload<MediaTooLongDiagnostic>();
        context.AddDiagnosticPayload<MediaTooShortDiagnostic>();
        context.AddDiagnosticPayload<MissingAudioStreamDiagnostic>();
        context.AddDiagnosticPayload<MissingSubtitleStreamDiagnostic>();
        context.AddDiagnosticPayload<MissingVideoStreamDiagnostic>();
        context.AddDiagnosticPayload<SubtitleBitrateTooHighDiagnostic>();
        context.AddDiagnosticPayload<SubtitleBitrateTooLowDiagnostic>();
        context.AddDiagnosticPayload<SubtitleCodecNotAllowedDiagnostic>();
        context.AddDiagnosticPayload<VideoBitrateTooHighDiagnostic>();
        context.AddDiagnosticPayload<VideoBitrateTooLowDiagnostic>();
        context.AddDiagnosticPayload<VideoCodecNotAllowedDiagnostic>();
        context.AddDiagnosticPayload<VideoFramerateNotAllowedDiagnostic>();
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
