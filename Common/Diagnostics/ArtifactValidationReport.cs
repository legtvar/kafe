using System;
using System.Collections.Immutable;

namespace Kafe.Diagnostics;

public record ArtifactValidationReport(
    Hrib ArtifactId,
    Hrib BlueprintId,
    DateTimeOffset ValidatedOn,
    ImmutableArray<Diagnostic> Diagnostics
);
