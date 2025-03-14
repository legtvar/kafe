using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafeCommon(this IServiceCollection services)
    {
        services.AddSingleton<KafeTypeRegistry>();
        services.AddSingleton<RequirementRegistry>();
        services.AddSingleton<ModRegistry>();
        return services;
    }
}
