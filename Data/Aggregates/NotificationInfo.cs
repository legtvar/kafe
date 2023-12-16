using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record NotificationInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    NotificationKind Kind,
    ImmutableArray<string> Recipients,
    [Hrib] string? ProjectId,
    [LocalizedString] ImmutableDictionary<string, string> Description,
    bool IsSent = false
) : IEntity;

public class NotificationInfoProjection : SingleStreamProjection<NotificationInfo>
{
    public NotificationInfoProjection()
    {
    }

    public static NotificationInfo Create(NotificationCreated e)
    {
        return new NotificationInfo
        (
            Id: e.NotificationId,
            CreationMethod: e.CreationMethod,
            Kind: e.Kind,
            Recipients: !e.Recipients.HasValue || e.Recipients.Value.IsDefault
                ? ImmutableArray.Create<string>()
                : e.Recipients.Value,
            ProjectId: e.ProjectId,
            Description: e.Description
        );
    }
    
    public NotificationInfo Apply(NotificationSent e, NotificationInfo n)
    {
        return n with { IsSent = true };
    }
}
