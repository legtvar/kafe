using System;
using System.Collections.Generic;

namespace Kafe;

public class PropertyTypeRegistry : SubtypeRegistryBase<PropertyTypeMetadata>
{
    public const string SubtypePrimary = "";

    public static readonly LocalizedString FallbackName = LocalizedString.Create(
        (Const.InvariantCulture, "artifact property of type '{0}'"),
        (Const.CzechCulture, "vlastnost artefaktu typu '{0}'")
    );
}

public static class PropertyTypeModContextExtensions
{
    public static KafeType AddArtifactProperty(
        this ModContext c,
        Type propertyType,
        PropertyRegistrationOptions? options = null
    )
    {
        var propertyTypeRegistry = c.RequireSubtypeRegistry<PropertyTypeMetadata>();
        options ??= PropertyRegistrationOptions.Default;
        options.Subtype ??= PropertyTypeRegistry.SubtypePrimary;

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            var typeName = propertyType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Property");
            typeName = Naming.WithoutSuffix(typeName, "PropertyMetadata");
            typeName = Naming.ToDashCase(typeName);
            options.Name = typeName;
        }

        options.HumanReadableName ??= LocalizedString.Format(
            PropertyTypeRegistry.FallbackName,
            options.Name
        );

        var kafeType = c.AddType(propertyType, options);
        propertyTypeRegistry.Register(new(
            KafeType: kafeType,
            DefaultRequirements: [.. options.DefaultRequirements]
        ));
        return kafeType;
    }

    public static KafeType AddArtifactProperty<TProperty>(
        this ModContext c,
        PropertyRegistrationOptions? options = null
    )
    {
        return c.AddArtifactProperty(typeof(TProperty), options);
    }

    public record PropertyRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly PropertyRegistrationOptions Default = new();

        public List<IRequirement> DefaultRequirements { get; set; } = [];
    }
}
