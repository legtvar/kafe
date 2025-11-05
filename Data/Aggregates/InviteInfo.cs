using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using Kafe.Data.Events;
using Marten.Events.Aggregation;
using Marten.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kafe.Data.Aggregates;

public record InviteInfo(
    [Hrib]
    string Id,
    CreationMethod CreationMethod,
    string EmailAddress,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn,
    ImmutableDictionary<string, Permission> Permissions
) : IEntity, ISoftDeleted
{
    public InviteInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        EmailAddress: Const.InvalidEmailAddress,
        CreatedOn: default,
        UpdatedOn: default,
        Permissions: ImmutableDictionary<string, Permission>.Empty
    )
    {
    }

    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class InviteInfoProjection : SingleStreamProjection<InviteInfo, string>
{
    public InviteInfoProjection()
    {
        IncludeType<InviteCreated>();
        IncludeType<InviteDestroyed>();
        IncludeType<InvitePermissionSet>();
    }

    protected static ILogger<InviteInfoProjection> Logger { get; set; } = NullLogger<InviteInfoProjection>.Instance;

    public override (InviteInfo?, ActionType) DetermineAction(
        InviteInfo? snapshot,
        string identity,
        IReadOnlyList<IEvent> events
    )
    {
        if (snapshot is null && events.HasNoEventsOfType<InviteCreated>())
        {
            return (null, ActionType.Nothing);
        }

        var action = ActionType.Store;
        var queue = new Queue<IEvent>(events);
        while (queue.Count > 0)
        {
            var @event = queue.Dequeue();
            switch (@event.Data)
            {
                case InviteCreated create:
                    snapshot = new InviteInfo(
                        Id: create.InviteId,
                        CreationMethod: create.CreationMethod,
                        EmailAddress: create.EmailAddress,
                        CreatedOn: @event.Timestamp,
                        UpdatedOn: @event.Timestamp,
                        Permissions: ImmutableDictionary<string, Permission>.Empty
                    );
                    break;

                case InviteDestroyed when snapshot is { Deleted: false }:
                    snapshot = snapshot with
                    {
                        Deleted = true,
                        UpdatedOn = @event.Timestamp
                    };
                    action = ActionType.StoreThenSoftDelete;
                    break;

                case InvitePermissionSet setEvent when snapshot is { Deleted: false }:
                    snapshot = snapshot with
                    {
                        UpdatedOn = @event.Timestamp,
                        Permissions = setEvent.Permission == Permission.None
                            ? snapshot.Permissions.Remove(setEvent.EntityId)
                            : snapshot.Permissions.SetItem(setEvent.EntityId, setEvent.Permission)
                    };
                    break;

                default:
                    Logger.LogWarning(
                        "Encountered an event that is invalid at the current point and will be ignored: {Event}.",
                        @event.Data
                    );
                    break;
            }
        }

        return (snapshot, action);
    }
}
