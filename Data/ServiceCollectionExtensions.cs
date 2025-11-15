using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events.Upcasts;
using Kafe.Data.Metadata;
using Kafe.Data.Options;
using Kafe.Data.Projections;
using Kafe.Data.Services;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafeData(this IServiceCollection services)
    {
        StoreOptions ConfigureMarten(IServiceProvider services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var environment = services.GetRequiredService<IHostEnvironment>();

            var options = services.GetRequiredService<IOptions<StorageOptions>>().Value;
            var mo = new StoreOptions();

            if (!string.IsNullOrEmpty(options.Schema))
            {
                mo.DatabaseSchemaName = options.Schema;
                mo.Events.DatabaseSchemaName = options.Schema;
            }

            mo.Connection(configuration.GetConnectionString("KAFE")
                ?? throw new ArgumentException("The KAFE connection string is missing!"));
            mo.Events.StreamIdentity = StreamIdentity.AsString;
            mo.Events.UseMandatoryStreamTypeDeclaration = true;
            mo.Events.MetadataConfig.UserNameEnabled = true;

            if (environment.IsDevelopment())
            {
                mo.AutoCreateSchemaObjects = AutoCreate.All;
                mo.CommandTimeout = 1_000_000;
            }

            mo.CreateDatabasesForTenants(c =>
            {
                c.MaintenanceDatabase(configuration.GetConnectionString("postgres")
                    ?? throw new ArgumentException("The postgres connection string is missing!"));
                c.ForTenant()
                    .CheckAgainstPgDatabase()
                    .WithOwner("postgres")
                    .WithEncoding("UTF-8")
                    .ConnectionLimit(-1);
            });

            mo.Projections.Add<AuthorInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<ArtifactInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<VideoShardInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<ImageShardInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<SubtitlesShardInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<BlendShardInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<NotificationInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<PlaylistInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<ProjectInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<ProjectGroupInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<VideoConversionInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<ArtifactDetailProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<AccountInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<OrganizationInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<RoleInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<InviteInfoProjection>(ProjectionLifecycle.Inline);
            mo.Projections.Add<EntityPermissionEventProjection>(
                ProjectionLifecycle.Async,
                ao =>
                {
                    ao.EnableDocumentTrackingByIdentity = true;
                    // NB: Since some of the projections query other perm infos, the events need to be processed
                    //     one by one.
                    ao.BatchSize = 1;
                    ao.TeardownDataOnRebuild = true;
                    ao.DeleteViewTypeOnTeardown<EntityPermissionInfo>();
                    ao.DeleteViewTypeOnTeardown<RoleMembersInfo>();
                });
            mo.Events.Upcast<AccountCapabilityAddedUpcaster>();
            mo.Events.Upcast<AccountCapabilityRemovedUpcaster>();
            mo.Events.Upcast<PlaylistVideoAddedUpcaster>();
            mo.Events.Upcast<PlaylistVideoRemovedUpcaster>();
            mo.Events.Upcast<TemporaryAccountCreatedUpcaster>();
            mo.Events.Upcast<TemporaryAccountClosedUpcaster>();
            mo.Events.Upcast<AccountPermissionUnsetUpcaster>();
            mo.UseNewtonsoftForSerialization();

            RegisterEmbeddedSql(mo);

            return mo;
        }


        var mce = services.AddMarten(ConfigureMarten)
            .ApplyAllDatabaseChangesOnStartup()
            .UseIdentitySessions()
            .InitializeWith<Corrector>()
            .InitializeWith<UserSeedData>()
            .AddAsyncDaemon(DaemonMode.Solo);

        services.AddSingleton<StorageService>();
        services.AddSingleton<EntityMetadataProvider>();

        services.AddScoped<AccountService>();
        services.AddScoped<ProjectGroupService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<AuthorService>();
        services.AddScoped<ArtifactService>();
        services.AddScoped<ShardService>();
        services.AddScoped<PlaylistService>();
        services.AddScoped<EntityService>();
        services.AddScoped<MigrationService>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<RoleService>();

        services.AddOptions<StorageOptions>()
            .BindConfiguration("Storage")
            .ValidateDataAnnotations();

        services.AddOptions<SeedOptions>()
            .BindConfiguration("Seed")
            .ValidateDataAnnotations();

        return services;
    }

    private static void RegisterEmbeddedSql(StoreOptions options)
    {
        // NB: Currently assumes all raw sql is a function

        var assembly = Assembly.GetExecutingAssembly();
        var sqlFiles = assembly.GetManifestResourceNames()
            .Where(n => Path.GetExtension(n) == ".sql");

        foreach (var sqlFile in sqlFiles)
        {
            using var stream = assembly.GetManifestResourceStream(sqlFile)
                ?? throw new NotSupportedException($"Embedded Sql '{sqlFile}' could not be found.");
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd().Replace("{databaseSchema}", options.DatabaseSchemaName);

            var objectName = Path.GetFileNameWithoutExtension(sqlFile);
            options.Storage.ExtendedSchemaObjects.Add(
                new Function(new PostgresqlObjectName(options.DatabaseSchemaName, objectName), contents));
        }
    }
}
