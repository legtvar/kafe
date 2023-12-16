using System;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record SystemDetailDto(
    string Name,
    ImmutableArray<string> BaseUrls,
    string Version,
    string Commit,
    DateTimeOffset CommitDate,
    DateTimeOffset RunningSince
);
