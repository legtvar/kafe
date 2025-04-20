using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record ShardInfo(
    [Hrib] string Id,
    LocalizedString Name,
    CreationMethod CreationMethod,
    DateTimeOffset CreatedAt,
    string MimeType,
    long FileLength,
    string? UploadFilename,
    KafeObject Metadata,
    ImmutableDictionary<string, KafeObject> Variants
) : IShard
{
    public static readonly ShardInfo Invalid = new();

    public ShardInfo() : this(
        Id: Hrib.InvalidValue,
        Name: LocalizedString.Empty,
        CreationMethod: CreationMethod.Unknown,
        CreatedAt: default,
        FileLength: -1,
        UploadFilename: null,
        MimeType: Const.InvalidMimeType,
        Metadata: KafeObject.Invalid,
        Variants: ImmutableDictionary<string, KafeObject>.Empty
    )
    {
    }

    IReadOnlyDictionary<string, KafeObject> IShard.Variants => Variants;

    Hrib IEntity.Id => Id;
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
            Name: e.Data.Name ?? LocalizedString.Empty,
            CreationMethod: e.Data.CreationMethod,
            CreatedAt: e.Timestamp,
            FileLength: e.Data.FileLength ?? -1,
            UploadFilename: e.Data.UploadFilename,
            MimeType: e.Data.MimeType ?? Const.InvalidMimeType,
            Metadata: e.Data.Metadata,
            Variants: ImmutableDictionary<string, KafeObject>.Empty
        );
    }
    
    public ShardInfo Apply(ShardInfoChanged e, ShardInfo s)
    {
        return s with
        {
            Name = e.Name ?? s.Name,
            FileLength = e.FileLength ?? s.FileLength,
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
