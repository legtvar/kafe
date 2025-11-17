using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;
using Marten.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JasperFx.Events;

namespace Kafe.Data.Aggregates;

public record AccountInfo(
    [Hrib] string Id,

    CreationMethod CreationMethod,

    AccountKind Kind,

    string? IdentityProvider,

    [property: UniqueIndex(IndexType = UniqueIndexType.Computed)]
    [property:Sortable]
    string EmailAddress,

    [property:Sortable]
    string PreferredCulture,

    [property:Sortable]
    string? Name,

    [property:Sortable]
    string? Uco,

    [property:Sortable]
    string? Phone,

    DateTimeOffset RefreshedOn,

    ImmutableDictionary<string, Permission> Permissions,

    ImmutableArray<string> RoleIds
) : IEntity
{
    public static readonly AccountInfo Invalid = new();

    public AccountInfo() : this(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Kind: AccountKind.Unknown,
        IdentityProvider: null,
        EmailAddress: Const.InvalidEmailAddress,
        PreferredCulture: Const.InvariantCultureCode,
        Name: null,
        Uco: null,
        Phone: null,
        RefreshedOn: default,
        Permissions: ImmutableDictionary<string, Permission>.Empty,
        RoleIds: []
    )
    {
    }


    /// <summary>
    /// Creates a bare-bones but valid <see cref="AccountInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static AccountInfo Create(string emailAddress, string? preferredCulture = null)
    {
        return new AccountInfo() with
        {
            Id = Hrib.EmptyValue,
            EmailAddress = emailAddress,
            PreferredCulture = preferredCulture ?? Const.InvariantCultureCode
        };
    }
}

public class AccountInfoProjection : SingleStreamProjection<AccountInfo, string>
{
    public AccountInfoProjection()
    {
    }

    public static AccountInfo Create(AccountCreated e)
    {
        return new(
            Id: e.AccountId,
            CreationMethod: e.CreationMethod,
            Kind: AccountKind.Unknown,
            IdentityProvider: null,
            EmailAddress: e.EmailAddress,
            PreferredCulture: e.PreferredCulture,
            Name: null,
            Uco: null,
            Phone: null,
            RefreshedOn: default,
            Permissions: ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, Permission>(e.AccountId, Permission.Read | Permission.Write)
            }),
            RoleIds: ImmutableArray<string>.Empty
        );
    }

    public AccountInfo Apply(IEvent<ExternalAccountAssociated> e, AccountInfo a)
    {
        return a with
        {
            Kind = AccountKind.External,
            IdentityProvider = e.Data.IdentityProvider,
            Name = e.Data.Name,
            Uco = e.Data.Uco,
            RefreshedOn = e.Timestamp
        };
    }

    public AccountInfo Apply(AccountInfoChanged e, AccountInfo a)
    {
        return a with
        {
            PreferredCulture = e.PreferredCulture ?? a.PreferredCulture,
            Name = e.Name ?? a.Name,
            Phone = e.Phone ?? a.Phone,
            Uco = e.Uco ?? a.Uco
        };
    }

    public AccountInfo Apply(AccountPermissionSet e, AccountInfo a)
    {
        // NB: Once upon a time, we used "*" to identify the whole system. Then we changed it, but we've got total of
        //     THREE AccountPermissionSet events in the DB that would break this method. Implementing an upcast that
        //     would have to run on any past or future AccountPermissionSet seems inefficient so I just added this `if`.
        //     We all have to live with our mistakes.
        if (e.EntityId == "*")
        {
            e = e with
            {
                EntityId = Hrib.SystemValue
            };
        }

        if (a.Permissions is null)
        {
            a = a with { Permissions = ImmutableDictionary<string, Permission>.Empty };
        }

        return a with
        {
            Permissions = e.Permission == Permission.None
                ? a.Permissions.Remove(e.EntityId)
                : a.Permissions.SetItem(e.EntityId, e.Permission)
        };
    }

    public AccountInfo Apply(AccountRoleSet e, AccountInfo a)
    {
        return a with
        {
            RoleIds = a.RoleIds.Contains(e.RoleId) ? a.RoleIds : a.RoleIds.Add(e.RoleId)
        };
    }

    public AccountInfo Apply(AccountRoleUnset e, AccountInfo a)
    {
        return a with
        {
            RoleIds = a.RoleIds.Contains(e.RoleId) ? a.RoleIds.Remove(e.RoleId) : a.RoleIds
        };
    }
}
