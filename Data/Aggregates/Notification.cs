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

    public Notification Create(IEvent<NotificationCreated> e)
    {
        return new Notification
        (
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Kind: e.Data.Kind,
            Recipients: !e.Data.Recipients.HasValue || e.Data.Recipients.Value.IsDefault
                ? ImmutableArray.Create<string>()
                : e.Data.Recipients.Value,
            ProjectId: e.Data.ProjectId,
            VideoId: e.Data.VideoId,
            Description: e.Data.Description
        );
    }
    
    public Notification Apply(NotificationSent e, Notification n)
    {
        return n with { IsSent = true };
    }
}
