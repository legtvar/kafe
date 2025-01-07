using Kafe.Data.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;
using System;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ArtifactInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    DateTimeOffset AddedOn,
    ImmutableDictionary<string, ArtifactProperty> Properties
) : IEntity
{
    public static readonly ArtifactInfo Invalid = new();

    public ArtifactInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        AddedOn: default,
        Properties: ImmutableDictionary<string, ArtifactProperty>.Empty
    )
    {
    }

    /// <summary>
    /// Creates a bare-bones but valid <see cref="ArtifactInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static ArtifactInfo Create(LocalizedString name)
    {
        return new ArtifactInfo() with
        {
            Id = Hrib.EmptyValue,
            Name = name
        };
    }
}

public record ArtifactProperty(
    KafeType Type,
    object Value
);

public class ArtifactInfoProjection : SingleStreamProjection<ArtifactInfo>
{
    public static ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            AddedOn: e.AddedOn,
            Properties: ImmutableDictionary<string, ArtifactProperty>.Empty
        );
    }

    public ArtifactInfo Apply(ArtifactInfoChanged e, ArtifactInfo a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            AddedOn = e.AddedOn ?? a.AddedOn
        };
    }

    public ArtifactInfo Apply(ArtifactPropertiesSet e, ArtifactInfo a)
    {
        var builder = a.Properties.ToBuilder();
        foreach (var (key, setter) in e.Properties)
        {
            if (setter.Value is null)
            {
                builder.Remove(key);
                continue;
            }

            if (setter.Type is null || setter.Type.Value == KafeType.Invalid)
            {
                throw new InvalidOperationException("An artifact property cannot be set without a valid KAFE type.");
            }

            switch (setter.ExistingValueHandling)
            {
                case ArtifactExistingPropertyValueHandling.OverwriteExisting:
                    builder[key] = new(
                        Type: setter.Type.Value,
                        Value: setter.Value
                    );
                    break;

                case ArtifactExistingPropertyValueHandling.KeepExisting:
                    if (builder.ContainsKey(key))
                    {
                        continue;
                    }

                    builder[key] = new(
                        Type: setter.Type.Value,
                        Value: setter.Value
                    );
                    break;

                case ArtifactExistingPropertyValueHandling.Append:
                    if (!builder.TryGetValue(key, out var property))
                    {
                        builder[key] = new(
                            Type: setter.Type.Value,
                            Value: setter.Value
                        );
                        continue;
                    }

                    if (property.Type.IsArray && (property.Type.GetElementType() == setter.Type.Value))
                    {
                        // builder[key] = property with
                        // {
                        //     Value = (List)property.Value
                        // };
                        throw new NotImplementedException();
                    }

                    break;

                default:
                    throw new NotSupportedException($"{setter.ExistingValueHandling} is not a supported "
                        + $"{nameof(ArtifactExistingPropertyValueHandling)} value.");
            }
        }

        return a with { Properties = builder.ToImmutable() };
    }
}
