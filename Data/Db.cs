using Kafe.Data.Aggregates;
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

            options.Projections.Add<AuthorInfoProjection>();
            options.Projections.Add<ArtifactInfoProjection>();
            options.Projections.Add<VideoShardInfoProjection>();
            options.Projections.Add<ImageShardInfoProjection>();
            options.Projections.Add<SubtitlesShardInfoProjection>();
            options.Projections.Add<NotificationInfoProjection>();
            options.Projections.Add<PlaylistInfoProjection>();
            options.Projections.Add<ProjectInfoProjection>();
            options.Projections.Add<ProjectGroupProjection>();
            options.Projections.Add<VideoConversionInfoProjection>();
            options.Projections.Add<ArtifactDetailProjection>(ProjectionLifecycle.Inline);
            options.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);
        }

        services.AddMarten(ConfigureMarten);
    }
}
