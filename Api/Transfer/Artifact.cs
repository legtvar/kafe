using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ArtifactDto(
    string Id,
    string ProjectId,
    ImmutableArray<ShardListDto> Shards);
