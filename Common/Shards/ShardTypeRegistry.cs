using System;
using System.Collections.Generic;

namespace Kafe;

public class ShardTypeRegistry : SubtypeRegistryBase<ShardTypeMetadata>
{
}

public static class ShardTypeModContextExtensions
{
    public const string SubtypePrimary = "shard";

    public static KafeType AddShard(
        this ModContext c,
        Type shardType,
        ShardRegistrationOptions? options = null
    )
    {
        var shardTypeRegistry = c.RequireSubtypeRegistry<ShardTypeMetadata>();
        options ??= ShardRegistrationOptions.Default;
        options.Subtype ??= SubtypePrimary;

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            var typeName = shardType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Shard");
            typeName = Naming.WithoutSuffix(typeName, "ShardMetadata");
            typeName = Naming.ToDashCase(typeName);
            options.Name = typeName;
        }

        var kafeType = c.AddType(shardType, options);
        shardTypeRegistry.Register(new(
            KafeType: kafeType,
            AnalyzerTypes: [.. options.AnalyzerTypes]
        ));
        return kafeType;
    }

    public static KafeType AddShard<T>(
        this ModContext c,
        ShardRegistrationOptions? options = null
    )
    {
        return c.AddShard(typeof(T), options);
    }

    public record ShardRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly ShardRegistrationOptions Default = new();

        public List<Type> AnalyzerTypes { get; set; } = [];
    }
}
