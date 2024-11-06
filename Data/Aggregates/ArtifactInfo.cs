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
    DateTimeOffset AddedOn
) : IEntity
{
    public static readonly ArtifactInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        AddedOn: default
    );

    public ArtifactInfo() : this(Invalid)
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
    public static ArtifactInfo Create(ArtifactCreated e)
    {
        return new ArtifactInfo(
            Id: e.ArtifactId,
            CreationMethod: e.CreationMethod,
            Name: e.Name,
            AddedOn: e.AddedOn);
    }

    public ArtifactInfo Apply(ArtifactInfoChanged e, ArtifactInfo a)
    {
        return a with
        {
            Name = e.Name ?? a.Name,
            AddedOn = e.AddedOn ?? a.AddedOn
        };
    }
}
