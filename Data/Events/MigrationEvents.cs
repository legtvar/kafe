using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record MigrationUndergone(
    [Hrib] string MigrationId,
    [Hrib] string EntityId,
    string OriginalStorageName,
    string OriginalId,
    ImmutableDictionary<string, string>? MigrationMetadata
);

public record MigrationAmended(
    [Hrib] string MigrationId,
    string? OriginalStorageName,
    string? OriginalId,
    ImmutableDictionary<string, string>? MigrationMetadata
);

public record MigrationRetargeted(
    [Hrib] string MigrationId,
    string RetargetedEntityId
);
