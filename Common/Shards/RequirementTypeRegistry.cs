using System;
using System.Collections.Generic;

namespace Kafe;

public class RequirementTypeRegistry : SubtypeRegistryBase<RequirementTypeMetadata>
{
}

public static class RequirementTypeModContextExtensions
{
    public const string SubtypePrimary = "req";

    public static KafeType AddRequirement(
        this ModContext c,
        Type requirementType,
        RequirementRegistrationOptions? options = null
    )
    {
        var requirementTypeRegistry = c.RequireSubtypeRegistry<RequirementTypeMetadata>();
        options ??= RequirementRegistrationOptions.Default;
        options.Subtype ??= SubtypePrimary;

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            var typeName = requirementType.Name;
            typeName = Naming.WithoutSuffix(typeName, "Requirement");
            typeName = Naming.ToDashCase(typeName);
            options.Name = typeName;
        }

        var kafeType = c.AddType(requirementType, options);
        requirementTypeRegistry.Register(new(
            KafeType: kafeType,
            HandlerTypes: [.. options.HandlerTypes]
        ));
        return kafeType;
    }

    public static KafeType AddRequirement<TRequirement>(
        this ModContext c,
        RequirementRegistrationOptions? options = null
    ) where TRequirement : IRequirement
    {
        return c.AddRequirement(typeof(TRequirement), options);
    }

    public record RequirementRegistrationOptions : ModContext.KafeTypeRegistrationOptions
    {
        public static new readonly RequirementRegistrationOptions Default = new();

        public List<Type> HandlerTypes { get; set; } = [];
    }
}
