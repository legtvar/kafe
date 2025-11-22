using System;
using System.Collections.Generic;

namespace Kafe.Data.Options;

public record VideoConversionOptions
{
    public bool IsDry { get; set; } = false;

    public TimeSpan PollWaitTime { get; set; } = TimeSpan.FromHours(1);

    public List<TimeSpan> FilterRanges { get; set; } =
    [
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(7),
        TimeSpan.FromDays(30),
        TimeSpan.FromDays(365)
    ];
}
