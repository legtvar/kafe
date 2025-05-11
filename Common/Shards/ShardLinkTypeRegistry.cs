using System;
using System.Collections.Immutable;

namespace Kafe;

public record ShardLinkTypeMetadata(
    KafeType KafeType,
    Type DotnetType
) : ISubtypeMetadata;

public class ShardLinkTypeRegistry : SubtypeRegistryBase<ShardLinkTypeMetadata>
{
    public const string SubtypePrimary = "shard-link";

    public static readonly LocalizedString FallbackName = LocalizedString.Create(
        (Const.InvariantCulture, "shard link of type '{0}'"),
        (Const.CzechCulture, "střípkový vztah typu '{0}'")
    );
}

public static class ShardLinkTypeModContextExtensions
{
    public static KafeType AddShardLink(
        this ModContext c,
        Type shardLinkType,
        ShardLinkRegistrationOptions? options = null
    )
    {
        var shardLinkTypeRegistry = c.RequireSubtypeRegistry<ShardLinkTypeMetadata>();
        options ??= ShardLinkRegistrationOptions.Default;
        options.Subtype ??= ShardTypeRegistry.SubtypePrimary;
        
        if (shardLinkType.IsAssignableTo(typeof(IShardLinkMetadata)))
        {
            options.Moniker ??= shardLinkType.GetStaticPropertyValue<string>(
                propertyName: nameof(IShardLinkMetadata.Moniker),
                isRequired: false,
                allowNull: true
            );
        }

        if (string.IsNullOrWhiteSpace(options.Moniker))
        {
            var typeName = shardLinkType.Name;
            typeName = Naming.WithoutSuffix(typeName, "ShardLink");
            typeName = Naming.ToDashCase(typeName);
            options.Moniker = typeName;
        }

        options.HumanReadableName ??= LocalizedString.Format(
            ShardTypeRegistry.FallbackName,
            options.Moniker
        );

        var kafeType = c.AddType(shardLinkType, options);
        shardLinkTypeRegistry.Register(new(
            KafeType: kafeType,
            DotnetType: shardLinkType
        ));
        return kafeType;
    }

    public static KafeType AddShardLink<T>(
        this ModContext c,
        ShardLinkRegistrationOptions? options = null
    )
    {
        return c.AddShardLink(typeof(T), options);
    }

    public record ShardLinkRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly ShardLinkRegistrationOptions Default = new();
    }
}
