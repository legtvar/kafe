using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kafe;

public sealed record class ModContext
{
    public ModContext(string name, KafeTypeRegistry typeRegistry)
    {
        Name = name;
        TypeRegistry = typeRegistry;
    }

    /// <summary>
    /// A short name for the mod (e.g., core, media, etc.).
    /// </summary>
    /// <remarks>
    /// May contain only lower-case letters, numbers, or '-'.
    /// Generated based on thre presence of a <see cref="ModAttribute"/> or generated automatically.
    /// </remarks>
    public string Name { get; }

    public KafeTypeRegistry TypeRegistry { get; }

    public ModContext AddType(Type type, KafeTypeRegistrationOptions? options = null)
    {
        options ??= KafeTypeRegistrationOptions.Default;

        var kafeType = new KafeType(
            mod: Name,
            primary: options.Name ?? Naming.ToDashCase(type.Name),
            secondary: null,
            isArray: false);
        TypeRegistry.Register(
            kafeType,
            new KafeTypeMetadata
            {
                Converter = options.Converter,
                Usage = options.Usage,

            })
    }

    public ModMetadata BuildMetadata()
    {
        return new ModMetadata
        {

        };
    }

    public record KafeTypeRegistrationOptions
    {
        public static readonly KafeTypeRegistrationOptions Default = new()
        {
            Usage = KafeTypeUsage.ArtifactProperty
        };

        public KafeTypeUsage Usage { get; set; }

        public string? Name { get; set; }

        public JsonConverter? Converter { get; set; }

        public List<IRequirement> DefaultRequirements { get; set; } = [];
    }
}
