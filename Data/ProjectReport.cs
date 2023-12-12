using System;
using System.Collections.Immutable;

namespace Kafe.Data;

public record ProjectReport(
    Hrib ProjectId,
    DateTimeOffset ValidatedOn,
    ImmutableArray<Diagnostic> Diagnostics
);
