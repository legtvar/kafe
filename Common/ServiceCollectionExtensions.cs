using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kafe;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafe(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        Action<KafeBrewingOptions> configureOptions
    )
    {
        services.AddSingleton<ShardAnalysisFactory>();
        services.AddSingleton<FileExtensionMimeMap>();

        var options = new KafeBrewingOptions(services);
        options.AddSubtypeRegistry(new RequirementTypeRegistry());
        options.AddSubtypeRegistry(new ShardTypeRegistry());
        options.AddSubtypeRegistry(new PropertyTypeRegistry());
        options.AddSubtypeRegistry(new DiagnosticDescriptorRegistry());
        options.AddFormatter(new FileLengthFormatter());
        options.AddFormatter(new BitrateFormatter());

        configureOptions(options);

        foreach (var mod in options.Mods)
        {
            mod.ConfigureOptions(options);
        }

        var typeRegistry = new KafeTypeRegistry(options.SubtypeRegistries);
        var modRegistry = new ModRegistry(
            typeRegistry: typeRegistry,
            configuration: configuration,
            hostEnvironment: hostEnvironment
        );
        services.AddSingleton(typeRegistry);
        services.AddSingleton(modRegistry);
        services.AddSingleton<KafeObjectFactory>();

        foreach (var mod in options.Mods)
        {
            var modName = (string?)mod.GetType().GetProperty(nameof(IMod.Moniker))!.GetValue(null);
            if (string.IsNullOrWhiteSpace(modName))
            {
                throw new InvalidOperationException($"Mod '{mod.GetType()}' declares an empty name.");
            }

            modRegistry.Register(mod, services);
        }

        foreach (var subtypeRegistry in options.SubtypeRegistries.Values)
        {
            subtypeRegistry.Freeze();
        }
        modRegistry.Freeze();
        typeRegistry.Freeze();

        foreach (var mod in options.Mods)
        {
            mod.PostConfigure();
        }

        return services;
    }
}
