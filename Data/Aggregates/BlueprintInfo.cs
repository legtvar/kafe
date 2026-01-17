using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Kafe.Data.Aggregates;

public record BlueprintInfo(
    [Hrib]
    string Id,
    [LocalizedString]
    ImmutableDictionary<string, string> Name,
    [LocalizedString]
    ImmutableDictionary<string, string>? Description,
    ImmutableDictionary<string, BlueprintProperty> Properties,
    bool AllowAdditionalProperties
) : IBlueprint, IInvalidable<BlueprintInfo>
{
    public static BlueprintInfo Invalid { get; } = new BlueprintInfo(
        id: Hrib.Invalid,
        name: LocalizedString.Empty
    );

    private readonly Lazy<IReadOnlyDictionary<string, IBlueprintProperty>> lazyAbstractProperties
        = new(() =>
            Properties.Select(p => new KeyValuePair<string, IBlueprintProperty>(p.Key, p.Value)).ToImmutableDictionary()
        );

    public BlueprintInfo(
        Hrib id,
        LocalizedString name,
        LocalizedString? description = null,
        ImmutableDictionary<string, BlueprintProperty>? properties = null,
        bool allowAdditionalProperties = false
    ) : this(
        Id: id.ToString(throwOnInvalidAndEmpty: false),
        Name: name,
        Description: description,
        Properties: properties ?? ImmutableDictionary<string, BlueprintProperty>.Empty,
        AllowAdditionalProperties: allowAdditionalProperties
    )
    {
    }

    public bool IsValid => ((IEntity)this).Id.IsValid;

    Hrib IEntity.Id => Id;

    LocalizedString IBlueprint.Name => Name;

    IReadOnlyDictionary<string, IBlueprintProperty> IBlueprint.Properties
        => lazyAbstractProperties.Value;
}

public readonly record struct BlueprintProperty(
    [LocalizedString]
    ImmutableDictionary<string, string>? Name,
    [LocalizedString]
    ImmutableDictionary<string, string>? Description,
    ImmutableArray<KafeObject> Requirements
) : IBlueprintProperty
{
    public BlueprintProperty(
        LocalizedString? name = null,
        LocalizedString? description = null,
        ImmutableArray<KafeObject> requirements = default
    )
        : this(
            Name: name,
            Description: description,
            Requirements: requirements.IsDefaultOrEmpty ? [] : requirements
        )
    {
    }

    LocalizedString? IBlueprintProperty.Name => Name;

    LocalizedString? IBlueprintProperty.Description => Description;

    IReadOnlyList<KafeObject> IBlueprintProperty.Requirements => Requirements;
}
