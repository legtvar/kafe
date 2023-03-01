using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ArtifactDetailDto(
    Hrib Id,
    LocalizedString Name,
    ImmutableArray<ShardListDto> Shards,
    ImmutableArray<Hrib> ContainingProjectIds);

public record ArtifactCreationDto(
    LocalizedString Name,
    Hrib? ContainingProject
);