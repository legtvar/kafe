using System.Collections.Immutable;

namespace Kafe.Mate;

public record PigeonsTestResponse(
    ImmutableArray<PigeonsTestInfo>? Tests,
    string? Error = null
);
