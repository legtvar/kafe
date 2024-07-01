using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.CodeGeneration;
using Marten.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public record AccountInfo(
    [Hrib] string Id,
    CreationMethod CreationMethod,
    AccountKind Kind,
    string? IdentityProvider,

    [property: UniqueIndex(IndexType = UniqueIndexType.Computed)]
    string EmailAddress,

    string PreferredCulture,
    string? Name,
    string? Uco,
    string? Phone,
    string? SecurityStamp,
    DateTimeOffset RefreshedOn,
    ImmutableDictionary<string, Permission> Permissions,
    ImmutableArray<string> RoleIds
) : IEntity
{
    public AccountInfo() : this(Invalid)
    {
    }

    public static readonly AccountInfo Invalid = new(
        Id: Hrib.InvalidValue,
        CreationMethod: CreationMethod.Unknown,
        Kind: AccountKind.Unknown,
        IdentityProvider: null,
        EmailAddress: Const.InvalidEmailAddress,
        PreferredCulture: Const.InvariantCultureCode,
        Name: null,
        Uco: null,
        Phone: null,
        SecurityStamp: null,
        RefreshedOn: default,
        Permissions: ImmutableDictionary<string, Permission>.Empty,
        RoleIds: []
    );


    /// <summary>
    /// Creates a bare-bones but valid <see cref="AccountInfo"/>.
    /// </summary>
    [MartenIgnore]
    public static AccountInfo Create(string emailAddress)
    {
        return new AccountInfo() with
        {
            Id = Hrib.EmptyValue,
            EmailAddress = emailAddress
        };
    }
}

public class AccountInfoProjection : SingleStreamProjection<AccountInfo>
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
            SecurityStamp: null,
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
            SecurityStamp = null,
            Name = e.Data.Name,
            Uco = e.Data.Uco,
            RefreshedOn = e.Timestamp
        };
    }

    public AccountInfo Apply(IEvent<TemporaryAccountRefreshed> e, AccountInfo a)
    {
        return a with
        {
            Kind = AccountKind.Temporary,
            SecurityStamp = e.Data.SecurityStamp,
            RefreshedOn = e.Timestamp
        };
    }

    // // TODO: Add a "ticket" entity that will be identified by a guid, and will be one-time only instead of these
    // //       tokens.
    // public AccountInfo Apply(TemporaryAccountClosed e, AccountInfo a)
    // {
    //    return a with
    //    {
    //        SecurityStamp = null
    //    };
    // }

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
