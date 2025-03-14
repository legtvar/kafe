using Kafe.Media.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Media;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafeMedia(this IServiceCollection services)
    {
        services.AddSingleton<IMediaService, FFmpegCoreService>();
        services.AddSingleton<IImageService, ImageSharpService>();
        services.AddSingleton<IPigeonsTestQueue, PigeonsTestQueue>();
        services.AddSingleton<IMod, MediaMod>();
        return services;
    }
}
