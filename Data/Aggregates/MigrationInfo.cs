using System;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record MigrationInfo(
    [Hrib] string Id,
    [Hrib] string EntityId,
    string OriginalStorageName,
    string OriginalId,
    ImmutableDictionary<string, string> MigrationMetadata,
    DateTimeOffset CreatedOn,
    DateTimeOffset ChangedOn)
    : IEntity
{
    public static readonly MigrationInfo Invalid = new(
        Id: Hrib.InvalidValue,
        EntityId: string.Empty,
        OriginalStorageName: string.Empty,
        OriginalId: string.Empty,
        MigrationMetadata: null!,
        CreatedOn: default,
        ChangedOn: default);
}

public class MigrationInfoProjection : SingleStreamProjection<MigrationInfo>
{
    public MigrationInfoProjection()
    {
    }

    public static MigrationInfo Create(IEvent<MigrationUndergone> e)
    {
        return new MigrationInfo(
            Id: e.Data.MigrationId,
            EntityId: e.Data.EntityId,
            OriginalStorageName: e.Data.OriginalStorageName,
            OriginalId: e.Data.OriginalId,
            MigrationMetadata: e.Data.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty,
            CreatedOn: e.Timestamp,
            ChangedOn: e.Timestamp);
    }

    public MigrationInfo Apply(IEvent<MigrationAmended> e, MigrationInfo m)
    {
        return m with
        {
            OriginalStorageName = e.Data.OriginalStorageName ?? m.OriginalStorageName,
            OriginalId = e.Data.OriginalId ?? m.OriginalId,
            MigrationMetadata = m.MigrationMetadata.SetItems(
                e.Data.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty),
            ChangedOn = e.Timestamp
        };
    }

    public MigrationInfo Apply(IEvent<MigrationRetargeted> e, MigrationInfo m)
    {
        return m with
        {
            EntityId = e.Data.RetargetedEntityId,
            ChangedOn = e.Timestamp
        };
    }
}
