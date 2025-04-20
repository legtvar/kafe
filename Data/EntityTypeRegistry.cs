using System;
using Kafe.Data.Aggregates;

namespace Kafe.Data;

public record EntityTypeMetadata(
    KafeType KafeType,
    Type DotnetType
) : ISubtypeMetadata;

public class EntityTypeRegistry : SubtypeRegistryBase<EntityTypeMetadata>
{
    public const string SubtypePrimary = "entity";

    public static readonly LocalizedString FallbackName = LocalizedString.Create(
        (Const.InvariantCulture, "entity of type '{0}'"),
        (Const.CzechCulture, "entita typu '{0}'")
    );
}

public static class EntityTypeModContextExtensions
{
    public static EntityTypeMetadata AddEntityType(
        this ModContext c,
        Type entityType,
        EntityTypeRegistrationOptions? options = null
    )
    {
        var entityTypeRegistry = c.RequireSubtypeRegistry<EntityTypeMetadata>();
        options ??= EntityTypeRegistrationOptions.Default;
        options.Subtype ??= EntityTypeRegistry.SubtypePrimary;

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            var typeName = entityType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Entity");
            typeName = Naming.WithoutSuffix(typeName, "Info");
            typeName = Naming.ToDashCase(typeName);
            options.Name = typeName;
        }

        options.HumanReadableName ??= entityType.GetStaticPropertyValue<LocalizedString?>(
            propertyName: nameof(IEntity.Title),
            isRequired: false,
            allowNull: true
        );

        options.HumanReadableName ??= LocalizedString.Format(EntityTypeRegistry.FallbackName, options.Name);

        var kafeType = c.AddType(entityType, options);
        var entityMetadata = new EntityTypeMetadata(
            kafeType,
            entityType
        );
        entityTypeRegistry.Register(entityMetadata);
        return entityMetadata;
    }

    public static EntityTypeMetadata AddEntityType<TEntity>(
        this ModContext c,
        EntityTypeRegistrationOptions? options = null
    )
    where TEntity : class, IEntity
    {
        return c.AddEntityType(typeof(TEntity), options);
    }

    public record EntityTypeRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly EntityTypeRegistrationOptions Default = new();
    }
}
