using System;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record ArtifactDetailDto(
    Hrib Id,
    LocalizedString Name,
    DateTimeOffset AddedOn,
    ImmutableArray<ShardListDto> Shards,
    ImmutableArray<Hrib> ContainingProjectIds
);

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record ArtifactCreationDto(
    LocalizedString Name,
    DateTimeOffset? AddedOn,
    Hrib? ContainingProject,
    string? BlueprintSlot
);
