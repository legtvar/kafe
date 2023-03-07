using Kafe.Data.Aggregates;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Services.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Weasel.Core;

namespace Kafe.Data;

public static class Db
{
    public static void AddDb(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        void ConfigureMarten(StoreOptions options)
        {
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

            //options.Linq.FieldSources.Add(new LocalizedStringFieldSource());
            //options.Linq.MethodCallParsers.Add(new DummyMethodCallParser());
            //options.Linq.FieldSources.Add(new HribFieldSource());

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
        }

        services.AddMarten(ConfigureMarten);

        services.AddSingleton<IStorageService, DefaultStorageService>();

        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.Configure<SeedOptions>(configuration.GetSection("Seed"));
    }
}
