using System;
using System.Collections.Immutable;

namespace Kafe.Data.Documents;

/// <summary>
/// A document (not event-sourced) describing a set of corrective events appended to fix a mistake.
/// </summary>
public record EventCorrectionInfo(
    string Id,
    DateTimeOffset AppliedOn,
    ImmutableArray<string> AffectedStreams
)
{ }
