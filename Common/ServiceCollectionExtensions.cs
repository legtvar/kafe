using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafeCommon(this IServiceCollection services)
    {
        services.AddSingleton<KafeTypeRegistry>();
        services.AddSingleton<PropertyTypeRegistry>();
        services.AddSingleton<RequirementTypeRegistry>();
        services.AddSingleton<ShardTypeRegistry>();
        services.AddSingleton<ModRegistry>();
        return services;
    }
}
