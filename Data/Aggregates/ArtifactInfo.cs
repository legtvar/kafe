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
    ImmutableDictionary<string, KafeObject> Properties
) : IEntity
{
    public static readonly ArtifactInfo Invalid = new();

    public ArtifactInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        AddedOn: default,
        Properties: ImmutableDictionary<string, KafeObject>.Empty
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

public class ArtifactInfoProjection : SingleStreamProjection<ArtifactInfo>
{
    private readonly KafeObjectFactory kafeObjectFactory;

    public ArtifactInfoProjection(KafeObjectFactory kafeObjectFactory)
    {
        this.kafeObjectFactory = kafeObjectFactory;
    }

    public static ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            AddedOn: e.AddedOn,
            Properties: ImmutableDictionary<string, KafeObject>.Empty
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
            if (setter.Object is null)
            {
                builder.Remove(key);
                continue;
            }

            var newObject = setter.Object.Value;

            if (newObject.Type.IsInvalid || newObject.Type.IsDefault)
            {
                throw new InvalidOperationException("An artifact property cannot be set without a valid KAFE type.");
            }

            var oldObject = builder.GetValueOrDefault(key);

            // TODO: Report the error... somewhere.
            var setValue = kafeObjectFactory.Set(oldObject, newObject, setter.ExistingValueHandling, out var _);
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
