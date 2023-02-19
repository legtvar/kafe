using System.Collections.Immutable;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Notification(
    string Id,
    CreationMethod CreationMethod,
    NotificationKind Kind,
    ImmutableArray<string> Recipients,
    string? ProjectId,
    string? VideoId,
    LocalizedString Description,
    bool IsSent = false
) : IEntity;

public class NotificationProjection : SingleStreamAggregation<Notification>
{
    public NotificationProjection()
    {
    }

    public Notification Create(NotificationCreated e)
    {
        return new Notification
        (
            Id: e.NotificationId,
            CreationMethod: e.CreationMethod,
            Kind: e.Kind,
            Recipients: !e.Recipients.HasValue || e.Recipients.Value.IsDefault
                ? ImmutableArray.Create<string>()
                : e.Recipients.Value,
            ProjectId: e.ProjectId,
            VideoId: e.VideoId,
            Description: e.Description
        );
    }
    
    public Notification Apply(NotificationSent e, Notification n)
    {
        return n with { IsSent = true };
    }
}
