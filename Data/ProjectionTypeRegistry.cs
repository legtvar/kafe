using System;
using Marten.Events.Daemon;
using Marten.Events.Projections;

namespace Kafe.Data;

public record ProjectionTypeMetadata(
    KafeType KafeType,
    Type DotnetType,
    ProjectionLifecycle ProjectionLifecycle,
    Action<AsyncOptions>? AsyncOptions
) : ISubtypeMetadata;

public class ProjectionTypeRegistry : SubtypeRegistryBase<ProjectionTypeMetadata>
{
    public const string SubtypePrimary = "projection";

    public static readonly LocalizedString FallbackName = LocalizedString.Create(
        (Const.InvariantCulture, "event projection of type '{0}'"),
        (Const.CzechCulture, "eventová projekce typu '{0}'")
    );
}

public static class ProjectionTypeModContextExtensions
{
    public static ProjectionTypeMetadata AddProjection(
        this ModContext c,
        Type projectionType,
        ProjectionTypeRegistrationOptions? options = null
    )
    {

        var projectionTypeRegistry = c.RequireSubtypeRegistry<ProjectionTypeMetadata>();
        options ??= ProjectionTypeRegistrationOptions.Default;
        options.Subtype ??= EntityTypeRegistry.SubtypePrimary;

        if (string.IsNullOrWhiteSpace(options.Moniker))
        {
            var typeName = projectionType.Name;
            typeName = Naming.WithoutSuffix(typeName, "EventProjection");
            typeName = Naming.WithoutSuffix(typeName, "Projection");
            typeName = Naming.WithoutSuffix(typeName, "Aggregate");
            typeName = Naming.ToDashCase(typeName);
            options.Moniker = typeName;
        }

        options.HumanReadableName ??= LocalizedString.Format(EntityTypeRegistry.FallbackName, options.Moniker);

        var kafeType = c.AddType(projectionType, options);
        var projMetadata = new ProjectionTypeMetadata(
            kafeType,
            projectionType,
            options.ProjectionLifecycle,
            options.AsyncOptions
        );
        projectionTypeRegistry.Register(projMetadata);
        return projMetadata;
    }

    public static ProjectionTypeMetadata AddProjection<TProjection>(
        this ModContext c,
        ProjectionTypeRegistrationOptions? options = null
    )
    {
        return c.AddProjection(typeof(TProjection), options);
    }

    public record ProjectionTypeRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly ProjectionTypeRegistrationOptions Default = new();

        public ProjectionLifecycle ProjectionLifecycle { get; set; }

        public Action<AsyncOptions>? AsyncOptions { get; set; }
    }
}
