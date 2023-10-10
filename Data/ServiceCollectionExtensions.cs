using Kafe.Data.Aggregates;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Services.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weasel.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafeData(this IServiceCollection services)
    {
        StoreOptions ConfigureMarten(IServiceProvider services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var environment = services.GetRequiredService<IHostEnvironment>();

            var options = new StoreOptions();

            options.Connection(configuration.GetConnectionString("KAFE")
                ?? throw new ArgumentException("The KAFE connection string is missing!"));
            options.Events.StreamIdentity = StreamIdentity.AsString;
            if (environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
            options.CreateDatabasesForTenants(c =>
            {
                c.MaintenanceDatabase(configuration.GetConnectionString("postgres")
                    ?? throw new ArgumentException("The postgres connection string is missing!"));
                c.ForTenant()
                    .CheckAgainstPgDatabase()
                    .WithOwner("postgres")
                    .WithEncoding("UTF-8")
                    .ConnectionLimit(-1);
            });

            options.Projections.Add<AuthorInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<ArtifactInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<VideoShardInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<ImageShardInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<SubtitlesShardInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<NotificationInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<PlaylistInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<ProjectInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<ProjectGroupInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<VideoConversionInfoProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<ArtifactDetailProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<AccountInfoProjection>(ProjectionLifecycle.Inline);
            options.UseDefaultSerialization(serializerType: SerializerType.Newtonsoft);

            return options;
        }

        services.AddMarten(ConfigureMarten)
            .ApplyAllDatabaseChangesOnStartup()
            .UseIdentitySessions();

        services.AddSingleton<StorageService>();
        services.AddScoped<AccountService>();

        services.AddOptions<StorageOptions>()
            .BindConfiguration("Storage");

        services.AddOptions<SeedOptions>()
            .BindConfiguration("Seed");

        return services;
    }
}
