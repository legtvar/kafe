using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafeCore(this IServiceCollection services)
    {
        services.AddSingleton<IMod, CoreMod>();
        return services;
    }
}
