using System;
using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;

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
    public MigrationInfo() : this(Invalid)
    {
    }

    public static readonly MigrationInfo Invalid = new(
        Id: Hrib.InvalidValue,
        EntityId: Hrib.InvalidValue,
        OriginalStorageName: string.Empty,
        OriginalId: string.Empty,
        MigrationMetadata: ImmutableDictionary<string, string>.Empty,
        CreatedOn: default,
        ChangedOn: default);

    /// <summary>
    /// Creates a bare-bones but valid <see cref="MigrationInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static MigrationInfo Create(string originalStorageName, string originalId, Hrib migratedEntityId)
    {
        return new MigrationInfo
        {
            Id = Hrib.EmptyValue,
            OriginalStorageName = originalStorageName,
            OriginalId = originalId,
            EntityId = migratedEntityId.RawValue
        };
    }
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
