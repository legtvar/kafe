using System;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record ShardInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    DateTimeOffset CreatedAt,
    long? Size,
    string? UploadFilename,
    string? MimeType,
    KafeObject Metadata,
    ImmutableDictionary<string, KafeObject> Variants
) : IEntity
{
    public static readonly ShardInfo Invalid = new();

    public ShardInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        CreatedAt: default,
        Size: null,
        UploadFilename: null,
        MimeType: null,
        Metadata: KafeObject.Invalid,
        Variants: ImmutableDictionary<string, KafeObject>.Empty
    )
    {
    }
}

public class ShardInfoProjection : SingleStreamProjection<ShardInfo>
{
    private readonly KafeObjectFactory factory;

    public ShardInfoProjection(KafeObjectFactory factory)
    {
        this.factory = factory;
    }

    public static ShardInfo Create(IEvent<ShardCreated> e)
    {
        return new(
            Id: e.Data.ShardId,
            CreationMethod: e.Data.CreationMethod,
            CreatedAt: e.Timestamp,
            Size: e.Data.Size,
            UploadFilename: e.Data.UploadFilename,
            MimeType: e.Data.MimeType,
            Metadata: e.Data.Metadata,
            Variants: ImmutableDictionary<string, KafeObject>.Empty
        );
    }
    
    public ShardInfo Apply(ShardInfoChanged e, ShardInfo s)
    {
        return s with
        {
            Size = e.Size ?? s.Size,
            UploadFilename = e.UploadFilename ?? s.UploadFilename,
            MimeType = e.MimeType ?? s.MimeType
        };
    }

    public ShardInfo Apply(ShardVariantAdded e, ShardInfo s)
    {
        var existing = s.Variants.GetValueOrDefault(e.Name);
        // TODO: Report the error somewhere
        var newValue = factory.Set(existing, e.Metadata, e.ExistingValueHandling, out _);

        if (newValue is null)
        {
            return s;
        }

        return s with
        {
            Variants = s.Variants.SetItem(e.Name, newValue.Value)
        };
    }

    public ShardInfo Apply(ShardVariantRemoved e, ShardInfo s)
    {
        return s with
        {
            Variants = s.Variants.Remove(e.Name)
        };
    }

}
