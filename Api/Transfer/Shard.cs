using Kafe.Data;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ShardListDto(
    string Id,
    ShardKind Kind,
    ImmutableArray<string> Variants);
