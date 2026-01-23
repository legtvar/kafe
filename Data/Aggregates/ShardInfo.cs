using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using JasperFx.Events;
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
    KafeObject Payload,
    ImmutableHashSet<ShardLink> Links
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
        Payload: KafeObject.Invalid,
        Links: []
    )
    {
    }

    Hrib IEntity.Id => Id;

    IReadOnlySet<IShardLink> IShard.Links => Links.Cast<IShardLink>().ToImmutableHashSet();

    [JsonIgnore]
    public ImmutableDictionary<KafeType, ImmutableHashSet<ShardLink>> LinksByType { get; }
        = Links.GroupBy(l => l.Metadata.Type).ToImmutableDictionary(g => g.Key, g => g.ToImmutableHashSet());
}

public readonly record struct ShardLink(
    [Hrib] string Id,
    KafeObject Metadata
) : IShardLink
{
    Hrib IShardLink.Id => Id;
}

public class ShardInfoProjection : SingleStreamProjection<ShardInfo, string>
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
            Payload: e.Data.Payload,
            Links: []
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

    public ShardInfo Apply(ShardLinkAdded e, ShardInfo s)
    {
        return s with
        {
            Links = s.Links.Add(new(e.Id, e.Metadata))
        };
    }

    public ShardInfo Apply(ShardLinkRemoved e, ShardInfo s)
    {
        var links = s.Links.ToBuilder();
        if (e.Id is not null && e.Metadata is not null)
        {
            links.Remove(new ShardLink(e.Id, e.Metadata.Value));
        }
        if (e.Metadata is not null)
        {
            links.ExceptWith(links.Where(l => l.Metadata == e.Metadata));
        }
        else
        {
            links.ExceptWith(links.Where(l => l.Id == e.Id));
        }

        return s with
        {
            Links = links.ToImmutable()
        };
    }

}
