using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record MigrationInfo(
    [Hrib] string Id,
    [Hrib] string EntityId,
    string? OriginalStorageName,
    string OriginalId,
    ImmutableDictionary<string, string> MigrationMetadata);

public class MigrationInfoProjection : SingleStreamProjection<MigrationInfo>
{
    public MigrationInfoProjection()
    {
    }

    public static MigrationInfo Create(MigrationUndergone e)
    {
        return new MigrationInfo(
            Id: e.MigrationId,
            EntityId: e.EntityId,
            OriginalStorageName: e.OriginalStorageName,
            OriginalId: e.OriginalId,
            MigrationMetadata: e.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty);
    }

    public MigrationInfo Apply(MigrationAmended e, MigrationInfo m)
    {
        return m with
        {
            OriginalStorageName = e.OriginalStorageName ?? m.OriginalStorageName,
            OriginalId = e.OriginalId ?? m.OriginalId,
            MigrationMetadata = m.MigrationMetadata.SetItems(
                e.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty)
        };
    }

    public MigrationInfo Apply(MigrationRetargeted e, MigrationInfo m)
    {
        return m with
        {
            EntityId = e.RetargetedEntityId
        };
    }
}
