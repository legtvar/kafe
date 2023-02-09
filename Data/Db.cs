using Kafe.Data.Aggregates;
using Marten;
using Marten.Events;
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
        services.AddMarten(options =>
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

            options.Projections.Add<AuthorProjection>();
            options.Projections.Add<NotificationProjection>();
            options.Projections.Add<PlaylistProjection>();
            options.Projections.Add<ProjectProjection>();
            options.Projections.Add<ProjectGroupProjection>();
            options.Projections.Add<VideoConversionProjection>();
            options.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);
        });
    }
}
