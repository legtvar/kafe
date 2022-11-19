using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;

namespace Kafe.Data.Aggregates;

public record Notification(
    string Id,
    CreationMethod CreationMethod,
    NotificationKind Kind,
    List<string> Recipients,
    string? ProjectId,
    string? VideoId,
    string? Description,
    string? EnglishDescription,
    bool IsSent = false
);

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
            Recipients: e.Data.Recipients,
            ProjectId: e.Data.ProjectId,
            VideoId: e.Data.VideoId,
            Description: e.Data.Description,
            EnglishDescription: e.Data.EnglishDescription
        );
    }
    
    public Notification Apply(NotificationSent e, Notification n)
    {
        return n with { IsSent = true };
    }
}
