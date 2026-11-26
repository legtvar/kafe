using System;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ArtifactDetailDto(
    Hrib Id,
    LocalizedString Name,
    DateTimeOffset AddedOn,
    ImmutableArray<ShardListDto> Shards,
    ImmutableArray<ArtifactDetailContainingProjectDto> ContainingProjects
);

public record ArtifactDetailContainingProjectDto(
    Hrib Id,
    LocalizedString Name
);

public record ArtifactCreationDto(
    LocalizedString Name,
    DateTimeOffset? AddedOn,
    Hrib? ContainingProject,
    string? BlueprintSlot
);
