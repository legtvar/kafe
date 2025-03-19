using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kafe;

public class KafeTypeRegistry : IFreezable
{
    private readonly ConcurrentDictionary<KafeType, KafeTypeMetadata> types = new();

    private readonly ConcurrentDictionary<Type, KafeType> dotnetTypeMap = new();

    public bool IsFrozen { get; private set; }

    public IReadOnlyDictionary<KafeType, KafeTypeMetadata> Types { get; }

    public IReadOnlyDictionary<Type, KafeType> DotnetTypeMap { get; }

    public KafeTypeRegistry()
    {
        Types = types.AsReadOnly();
        DotnetTypeMap = dotnetTypeMap.AsReadOnly();
    }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public KafeTypeRegistry Register(KafeTypeMetadata metadata)
    {
        AssertUnfrozen();
        if (!types.TryAdd(metadata.KafeType, metadata))
        {
            throw new ArgumentException($"KafeType '{metadata.KafeType}' has been already registered.", nameof(metadata));
        }
        dotnetTypeMap.AddOrUpdate(metadata.DotnetType, metadata.KafeType, (_, _) => metadata.KafeType);
        return this;
    }

    private void AssertUnfrozen()
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("This KafeType registry is frozen and can no longer be modified.");
        }
    }

    public sealed record KafeTypeRegistrationOptions
    {
        public KafeTypeAccessibility Accessibility { get; set; } = KafeTypeAccessibility.Public;

        public List<IRequirement> DefaultRequirements { get; set; } = [];

        public JsonConverter? Converter { get; set; }
    }
}
