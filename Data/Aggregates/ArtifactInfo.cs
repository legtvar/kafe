using Kafe.Data.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record ArtifactInfo(
    [property:Hrib]
    string Id,

    CreationMethod CreationMethod,

    [property:Sortable]
    DateTimeOffset AddedOn,

    ImmutableDictionary<string, KafeObject> Properties
) : IArtifact, IInvalidable<ArtifactInfo>
{
    public static ArtifactInfo Invalid { get; } = new();

    public ArtifactInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        AddedOn: default,
        Properties: ImmutableDictionary<string, KafeObject>.Empty
    )
    {
    }

    Hrib IEntity.Id => Id;

    IReadOnlyDictionary<string, KafeObject> IArtifact.Properties => Properties;

    public LocalizedString Name => this.GetProperty<LocalizedString>(nameof(Name)) ?? Const.UnnamedArtifactName;

    public bool IsValid => ((IEntity)this).Id.IsValid;

    /// <summary>
    /// Creates a bare-bones but valid <see cref="ArtifactInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static ArtifactInfo Create()
    {
        return new ArtifactInfo { Id = Hrib.EmptyValue };
    }
}

public class ArtifactInfoProjection(
    KafeObjectFactory objectFactory
) : SingleStreamProjection<ArtifactInfo, string>
{
    public ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            AddedOn: e.AddedOn,
            Properties: objectFactory.WrapProperties((nameof(Name), e.Name))
        );
    }

    public ArtifactInfo Apply(ArtifactInfoChanged e, ArtifactInfo a)
    {
        return a with
        {
            AddedOn = e.AddedOn ?? a.AddedOn,
            Properties = e.Name is null
                ? a.Properties
                : a.Properties.SetItem(nameof(Name), objectFactory.Wrap(e.Name))
        };
    }

    public ArtifactInfo Apply(ArtifactPropertiesSet e, ArtifactInfo a)
    {
        var builder = a.Properties.ToBuilder();
        foreach (var (key, setter) in e.Properties)
        {
            if (setter.Object is null)
            {
                builder.Remove(key);
                continue;
            }

            var newObject = setter.Object.Value;

            if (!newObject.Type.IsValid || newObject.Type.IsDefault)
            {
                throw new InvalidOperationException("An artifact property cannot be set without a valid KAFE type.");
            }

            var oldObject = builder.GetValueOrDefault(key);

            // TODO: Report the error... somewhere.
            var setValue = objectFactory.Set(oldObject, newObject, setter.ExistingValueHandling, out var _);
            if (setValue is null)
            {
                builder.Remove(key);
            }
            else
            {
                builder[key] = setValue.Value;
            }
        }

        return a with { Properties = builder.ToImmutable() };
    }
}
