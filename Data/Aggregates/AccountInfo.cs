using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record AccountInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    AccountKind Kind,

    [property: UniqueIndex(IndexType = UniqueIndexType.Computed)]
    string EmailAddress,

    string PreferredCulture,
    string? SecurityStamp,
    DateTimeOffset RefreshedOn,
    ImmutableDictionary<string, Permission>? Permissions
) : IEntity;

public class AccountInfoProjection : SingleStreamProjection<AccountInfo>
{
    public AccountInfoProjection()
    {
    }

    public static AccountInfo Create(TemporaryAccountCreated e)
    {
        return new(
            Id: e.AccountId,
            CreationMethod: e.CreationMethod,
            Kind: AccountKind.Temporary,
            EmailAddress: e.EmailAddress,
            PreferredCulture: e.PreferredCulture,
            SecurityStamp: null,
            RefreshedOn: default,
            Permissions: ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, Permission>(e.AccountId, Permission.Read | Permission.Write)
            })
        );
    }

    public AccountInfo Apply(IEvent<TemporaryAccountRefreshed> e, AccountInfo a)
    {
        return a with
        {
            SecurityStamp = e.Data.SecurityStamp,
            RefreshedOn = e.Timestamp
        };
    }

    // TODO: Add a "ticket" entity that will be identified by a guid, and will be one-time only instead of these
    //       tokens.
    //public AccountInfo Apply(TemporaryAccountClosed e, AccountInfo a)
    //{
    //    return a with
    //    {
    //        SecurityStamp = null
    //    };
    //}

    public AccountInfo Apply(TemporaryAccountInfoChanged e, AccountInfo a)
    {
        return a with
        {
            PreferredCulture = e.PreferredCulture ?? a.PreferredCulture
        };
    }

    public AccountInfo Apply(AccountPermissionSet e, AccountInfo a)
    {
        if (a.Permissions is null)
        {
            a = a with { Permissions = ImmutableDictionary<string, Permission>.Empty };
        }

        return a with
        {
            Permissions = a.Permissions.SetItem(e.EntityId, e.Permission)
        };
    }

    public AccountInfo Apply(AccountPermissionUnset e, AccountInfo a)
    {
        if (a.Permissions is null)
        {
            a = a with { Permissions = ImmutableDictionary<string, Permission>.Empty };
        }

        return a with
        {
            Permissions = a.Permissions.Remove(e.EntityId)
        };
    }
}
