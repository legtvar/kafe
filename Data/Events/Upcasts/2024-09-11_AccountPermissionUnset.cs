#pragma warning disable 0618
using System;
using Marten.Services.Json.Transformations;

namespace Kafe.Data.Events
{
    [Obsolete("Use 'AccountPermissionSet' with 'Permission.None' instead.")]
    public record AccountPermissionUnset(
        [Hrib] string AccountId,
        [Hrib] string EntityId
    );
}

namespace Kafe.Data.Events.Upcasts
{
    internal class AccountPermissionUnsetUpcaster : EventUpcaster<AccountPermissionUnset, AccountPermissionSet>
    {
        protected override AccountPermissionSet Upcast(AccountPermissionUnset oldEvent)
        {
            return new AccountPermissionSet(
                AccountId: oldEvent.AccountId,
                EntityId: oldEvent.EntityId,
                Permission: Permission.None);
        }
    }
}
