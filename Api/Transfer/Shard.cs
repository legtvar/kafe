using Kafe.Data;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ShardListDto(
    Hrib Id,
    ShardKind Kind,
    ImmutableArray<string> Variants);
