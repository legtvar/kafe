using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events.Upcasts;
using Kafe.Data.Metadata;
using Kafe.Data.Options;
using Kafe.Data.Projections;
using Kafe.Data.Services;
using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Weasel.Postgresql;
using Weasel.Postgresql.Functions;

namespace Kafe.Data;

public class DataMod : IMod
{
    public static string Name { get; } = "data";

    public void ConfigureOptions(KafeBrewingOptions options)
    {
        options.AddSubtypeRegistry(new EntityTypeRegistry());
        options.AddSubtypeRegistry(new ProjectionTypeRegistry());
    }

    public void Configure(ModContext context)
    {
        ConfigureServices(context.Services);

        context.AddEntityType<ProjectInfo>();
        context.AddEntityType<ProjectGroupInfo>();
        context.AddEntityType<AuthorInfo>();
        context.AddEntityType<NotificationInfo>();
        context.AddEntityType<PlaylistInfo>();
        context.AddEntityType<VideoConversionInfo>();
        context.AddEntityType<AccountInfo>();
        context.AddEntityType<OrganizationInfo>();
        context.AddEntityType<RoleInfo>();
        context.AddEntityType<ArtifactInfo>();
        context.AddEntityType<ShardInfo>();
        context.AddEntityType<EntityPermissionInfo>();
        context.AddEntityType<RoleMembersInfo>();

        context.AddProjection<AuthorInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<NotificationInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<PlaylistInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<ProjectInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<ProjectGroupInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<VideoConversionInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<AccountInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<OrganizationInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<RoleInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<ArtifactInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<ShardInfoProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Inline
        });
        context.AddProjection<EntityPermissionEventProjection>(new()
        {
            ProjectionLifecycle = ProjectionLifecycle.Async,
            AsyncOptions = ao =>
            {
                ao.EnableDocumentTrackingByIdentity = true;
                // NB: Since some of the projections query other perm infos, the events need to be processed
                //     one by one.
                ao.BatchSize = 1;
                ao.TeardownDataOnRebuild = true;
                ao.DeleteViewTypeOnTeardown<EntityPermissionInfo>();
                ao.DeleteViewTypeOnTeardown<RoleMembersInfo>();
            }
        });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var mce = services.AddMarten(ConfigureMarten)
            .ApplyAllDatabaseChangesOnStartup()
            .UseIdentitySessions()
            .InitializeWith<Corrector>()
            .InitializeWith<UserSeedData>()
            .AddAsyncDaemon(DaemonMode.Solo);

        services.AddSingleton<StorageService>();
        services.AddSingleton<EntityMetadataProvider>();
        
        services.AddScoped<IKafeQuerySession, KafeDocumentSession>();
        services.AddScoped<IKafeDocumentSession, KafeDocumentSession>();

        services.AddScoped<AccountService>();
        services.AddScoped<ProjectGroupService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<AuthorService>();
        services.AddScoped<ArtifactService>();
        services.AddScoped<ShardService>();
        services.AddScoped<PlaylistService>();
        services.AddScoped<EntityService>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<RoleService>();
        services.AddScoped<VideoConversionService>();

        services.AddOptions<StorageOptions>()
            .BindConfiguration("Storage")
            .ValidateDataAnnotations();

        services.AddOptions<SeedOptions>()
            .BindConfiguration("Seed")
            .ValidateDataAnnotations();
    }

    public static StoreOptions ConfigureMarten(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var environment = services.GetRequiredService<IHostEnvironment>();
        var docRegistry = services.GetRequiredService<EntityTypeRegistry>();
        var projRegistry = services.GetRequiredService<ProjectionTypeRegistry>();

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
        mo.Events.UseIdentityMapForAggregates = true;

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

        foreach (var doc in docRegistry.Metadata.Values)
        {
            mo.RegisterDocumentType(doc.DotnetType);
        }

        foreach (var proj in projRegistry.Metadata.Values)
        {
            if (proj.DotnetType.IsAssignableTo(typeof(IProjection)))
            {
                mo.Projections.Add(
                    projection: (IProjection)ActivatorUtilities.CreateInstance(services, proj.DotnetType),
                    lifecycle: proj.ProjectionLifecycle,
                    asyncConfiguration: proj.AsyncOptions
                );
            }
            else if (proj.DotnetType.IsAssignableTo(typeof(EventProjection)))
            {
                mo.Projections.Add(
                    projection: (EventProjection)ActivatorUtilities.CreateInstance(services, proj.DotnetType),
                    lifecycle: proj.ProjectionLifecycle,
                    asyncConfiguration: proj.AsyncOptions
                );
            }
            else if (proj.DotnetType.BaseType?.GetGenericTypeDefinition() == typeof(GeneratedAggregateProjectionBase<>))
            {
                var docType = proj.DotnetType.BaseType.GetGenericArguments()[0];
                var addMethod = mo.Projections.GetType().GetMethod(
                    name: nameof(ProjectionOptions.Add),
                    genericParameterCount: 1,
                    types: null!,
                    bindingAttr: BindingFlags.Public
                );
                addMethod!.MakeGenericMethod(docType)
                    .Invoke(mo.Projections, [proj.ProjectionLifecycle, proj.AsyncOptions]);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Projection type '{proj.DotnetType}' is not of any supported kind of Marten projections.");
            }
        }

        mo.Events.Upcast<AccountCapabilityAddedUpcaster>();
        mo.Events.Upcast<AccountCapabilityRemovedUpcaster>();
        mo.Events.Upcast<PlaylistVideoAddedUpcaster>();
        mo.Events.Upcast<PlaylistVideoRemovedUpcaster>();
        mo.Events.Upcast<TemporaryAccountCreatedUpcaster>();
        mo.Events.Upcast<TemporaryAccountClosedUpcaster>();
        mo.Events.Upcast<AccountPermissionUnsetUpcaster>();
        mo.Events.Upcast<VideoShardCreatedUpcaster>();
        mo.Events.Upcast<VideoShardVariantAddedUpcaster>();
        mo.Events.Upcast<VideoShardVariantRemovedUpcaster>();
        mo.Events.Upcast<ImageShardVariantsAddedUpcaster>();
        mo.Events.Upcast<ImageShardCreatedUpcaster>();
        mo.Events.Upcast<ImageShardVariantsRemovedUpcaster>();
        mo.Events.Upcast<SubtitlesShardVariantsAddedUpcaster>();
        mo.Events.Upcast<SubtitlesShardCreatedUpcaster>();
        mo.Events.Upcast<SubtitlesShardVariantsRemovedUpcaster>();
        mo.Events.Upcast<BlendShardCreatedUpcaster>();
        mo.Events.Upcast<BlendShardVariantAddedUpcaster>();
        mo.Events.Upcast<BlendShardVariantRemovedUpcaster>();
        mo.UseNewtonsoftForSerialization();

        RegisterEmbeddedSql(mo);

        return mo;
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
