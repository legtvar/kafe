using System;
using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectGroupCreated(
    [Hrib] string ProjectGroupId,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name,
    Visibility Visibility
);

public record ProjectGroupInfoChanged(
    [Hrib] string ProjectGroupId,
    [LocalizedString] ImmutableDictionary<string, string>? Name = null,
    [LocalizedString] ImmutableDictionary<string, string>? Description = null,
    DateTimeOffset? Deadline = null);

public record ProjectGroupOpened(
    [Hrib] string ProjectGroupId
);

public record ProjectGroupClosed(
    [Hrib] string ProjectGroupId
);
