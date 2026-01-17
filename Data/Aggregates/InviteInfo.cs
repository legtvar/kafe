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
using Newtonsoft.Json;

namespace Kafe.Data.Aggregates;

public record InviteInfo(
    [Hrib]
    string Id,
    CreationMethod CreationMethod,
    string EmailAddress,
    string? PreferredCulture,
    InviteStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedOn,
    ImmutableDictionary<string, InvitePermissionEntry> Permissions
) : IEntity, ISoftDeleted
{
    public InviteInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        EmailAddress: Const.InvalidEmailAddress,
        PreferredCulture: null,
        Status: default,
        CreatedAt: default,
        UpdatedOn: default,
        Permissions: ImmutableDictionary<string, InvitePermissionEntry>.Empty
    )
    {
    }

    public static InviteInfo Create(string emailAddress, string? preferredCulture = null)
    {
        return new InviteInfo() with
        {
            Id = Hrib.EmptyValue,
            CreationMethod = CreationMethod.Api,
            EmailAddress = emailAddress,
            PreferredCulture = preferredCulture
        };
    }

    [JsonIgnore]
    public bool Deleted { get; set; }

    [JsonIgnore]
    public DateTimeOffset? DeletedAt { get; set; }

    Hrib IEntity.Id => Id;
}

public record InvitePermissionEntry(
    [Hrib]
    string EntityId,
    Permission Permission,
    [Hrib]
    string? InviterAccountId
);

public enum InviteStatus
{
    Created,
    Accepted,
    Canceled
}

public class InviteInfoProjection : SingleStreamProjection<InviteInfo, string>
{
    public InviteInfoProjection()
    {
        IncludeType<InviteCreated>();
        IncludeType<InviteCanceled>();
        IncludeType<InviteAccepted>();
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
                        PreferredCulture: create.PreferredCulture,
                        Status: default,
                        CreatedAt: @event.Timestamp,
                        UpdatedOn: @event.Timestamp,
                        Permissions: ImmutableDictionary<string, InvitePermissionEntry>.Empty
                    );
                    break;

                case InviteCanceled when snapshot is { Deleted: false }:
                    snapshot = snapshot with
                    {
                        Deleted = true,
                        UpdatedOn = @event.Timestamp,
                        Status = InviteStatus.Canceled
                    };
                    action = ActionType.StoreThenSoftDelete;
                    break;

                case InviteAccepted when snapshot is { Deleted: false }:
                    snapshot = snapshot with
                    {
                        Deleted = true,
                        UpdatedOn = @event.Timestamp,
                        Status = InviteStatus.Accepted
                    };
                    action = ActionType.StoreThenSoftDelete;
                    break;

                case InvitePermissionSet setEvent when snapshot is { Deleted: false }:
                    snapshot = snapshot with
                    {
                        UpdatedOn = @event.Timestamp,
                        Permissions = setEvent.Permission == Permission.None
                            ? snapshot.Permissions.Remove(setEvent.EntityId)
                            : snapshot.Permissions.SetItem(
                                setEvent.EntityId,
                                new InvitePermissionEntry(
                                    EntityId: setEvent.EntityId,
                                    Permission: setEvent.Permission,
                                    InviterAccountId: @event.UserName
                                )
                            )
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
