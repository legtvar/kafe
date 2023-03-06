using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Schema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Aggregates;

public record AccountInfo(
    string Id,
    CreationMethod CreationMethod,
    AccountKind Kind,

    [property: UniqueIndex(IndexType = UniqueIndexType.Computed)]
    string EmailAddress,

    string PreferredCulture,
    string? SecurityStamp,
    DateTimeOffset RefreshedOn,
    ImmutableHashSet<AccountCapability> Capabilities
);

public class AccountInfoProjection : SingleStreamAggregation<AccountInfo>
{
    public AccountInfoProjection()
    {
    }

    public AccountInfo Create(TemporaryAccountCreated e)
    {
        return new(
            Id: e.AccountId,
            CreationMethod: e.CreationMethod,
            Kind: AccountKind.Temporary,
            EmailAddress: e.EmailAddress,
            PreferredCulture: e.PreferredCulture,
            SecurityStamp: null,
            RefreshedOn: default,
            Capabilities: ImmutableHashSet<AccountCapability>.Empty
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

    public AccountInfo Apply(TemporaryAccountClosed e, AccountInfo a)
    {
        return a with
        {
            SecurityStamp = null
        };
    }

    public AccountInfo Apply(TemporaryAccountInfoChanged e, AccountInfo a)
    {
        return a with
        {
            PreferredCulture = e.PreferredCulture ?? a.PreferredCulture
        };
    }

    public AccountInfo Apply(AccountCapabilityAdded e, AccountInfo a)
    {
        return a with
        {
            Capabilities = a.Capabilities.Add(e.Capability)
        };
    }

    public AccountInfo Apply(AccountCapabilityRemoved e, AccountInfo a)
    {
        return a with
        {
            Capabilities = a.Capabilities.Remove(e.Capability)
        };
    }
}
