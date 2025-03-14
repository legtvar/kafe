using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Polygons;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafePolygons(this IServiceCollection services)
    {
        services.AddSingleton<IMod, PolygonsMod>();
        return services;
    }
}
