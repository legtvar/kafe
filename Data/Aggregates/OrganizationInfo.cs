using System;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Aggregates;

public record OrganizationInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    DateTimeOffset CreatedOn
) : IEntity
{
    public OrganizationInfo() : this(Invalid)
    {
        
    }

    public static readonly OrganizationInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Name: LocalizedString.CreateInvariant(Const.InvalidName),
        CreatedOn: default
    );

    /// <summary>
    /// Creates a bare-bones but valid <see cref="OrganizationInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static OrganizationInfo Create(LocalizedString name)
    {
        return new OrganizationInfo() with
        {
            Id = Hrib.EmptyValue,
            Name = name
        };
    }
}

public class OrganizationInfoProjection : SingleStreamProjection<OrganizationInfo>
{
    public OrganizationInfoProjection()
    {
    }

    public static OrganizationInfo Create(IEvent<OrganizationEstablished> e)
    {
        return new OrganizationInfo(
            Id: e.Data.OrganizationId,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            CreatedOn: e.Timestamp);
    }

    public OrganizationInfo Apply(OrganizationInfoChanged e, OrganizationInfo o)
    {
        return o with
        {
            Name = e.Name ?? o.Name
        };
    }
}
