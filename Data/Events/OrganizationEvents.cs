using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record OrganizationEstablished(
    [Hrib] string OrganizationId,
    CreationMethod CreationMethod,
    [LocalizedString] ImmutableDictionary<string, string> Name
);

public record OrganizationInfoChanged(
    [Hrib] string OrganizationId,
    [LocalizedString] ImmutableDictionary<string, string>? Name
);
