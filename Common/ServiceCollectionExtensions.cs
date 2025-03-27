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
        services.AddSingleton<ShardFactory>();
        return services;
    }

    public static IServiceCollection AddKafeMod<TMod>(this IServiceCollection services)
        where TMod : IMod, new()
    {
        var mod = new TMod();
        mod.ConfigureServices(services);
        services.AddSingleton<IMod>(mod);
        services.AddSingleton(typeof(TMod), mod);
        return services;
    }
}
