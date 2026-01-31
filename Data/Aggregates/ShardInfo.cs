using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using JasperFx.Events;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;

namespace Kafe.Data.Aggregates;

public record ShardInfo(
    [Hrib]
    string Id,
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

    [MartenIgnore]
    public static ShardInfo Create(LocalizedString name)
    {
        return new ShardInfo()
        {
            Id = Hrib.EmptyValue,
            Name = name
        };
    }

    Hrib IEntity.Id => Id;

    IReadOnlySet<IShardLink> IShard.Links => Links.Cast<IShardLink>().ToImmutableHashSet();

    [JsonIgnore]
    public ImmutableDictionary<KafeType, ImmutableHashSet<IShardLinkPayload>> LinksByType { get; }
        = Links.GroupBy(l => l.Payload.Type).ToImmutableDictionary(
            g => g.Key,
            g => g.Select(l => (IShardLinkPayload)l.Payload.Value).ToImmutableHashSet()
        );
}

public readonly record struct ShardLink(
    [Hrib]
    string DestinationId,
    KafeObject Payload
) : IShardLink
{
    Hrib IShardLink.DestinationId => DestinationId;
}

public class ShardInfoProjection(
    KafeObjectFactory factory
) : SingleStreamProjection<ShardInfo, string>
{
    private readonly KafeObjectFactory factory = factory;

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
            Links = s.Links.Add(new ShardLink(e.DestinationShardId, e.LinkPayload))
        };
    }

    public ShardInfo Apply(ShardLinkRemoved e, ShardInfo s)
    {
        var links = s.Links.ToBuilder();
        if (e.DestinationShardId is not null && e.LinkPayload is not null)
        {
            links.Remove(new ShardLink(e.DestinationShardId, e.LinkPayload.Value));
        }

        if (e.LinkPayload is not null)
        {
            links.ExceptWith(links.Where(l => l.Payload == e.LinkPayload));
        }
        else if (e.DestinationShardId is not null)
        {
            links.ExceptWith(links.Where(l => l.DestinationId == e.DestinationShardId));
        }

        return s with
        {
            Links = links.ToImmutable()
        };
    }
}
