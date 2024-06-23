using System;

namespace Kafe.Data.Documents;

public enum EventCorrectionStatus
{
    Applied,
    Reverted
}

/// <summary>
/// A document (not event-sourced) describing a set of corrective events appended to fix a mistake.
/// </summary>
/// <param name="CustomData">Any data the correction decided to store to, for example, revert itself later.</param>
public record EventCorrectionInfo<T>(
    string Id,
    DateTimeOffset AppliedOn,
    T? CustomData
)
{
    public EventCorrectionStatus Status { get; set; } = EventCorrectionStatus.Applied;
}
