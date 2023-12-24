#pragma warning disable 0618

using System;
using Marten.Services.Json.Transformations;

namespace Kafe.Data.Events
{
    [Obsolete("Use 'AccountCreated' instead.")]
    public record TemporaryAccountCreated(
        [Hrib] string AccountId,
        CreationMethod CreationMethod,
        string EmailAddress,
        string PreferredCulture
    );
}

namespace Kafe.Data.Events.Upcasts
{
    internal class TemporaryAccountCreatedUpcaster : EventUpcaster<TemporaryAccountCreated, AccountCreated>
    {
        protected override AccountCreated Upcast(TemporaryAccountCreated oldEvent)
        {
            return new AccountCreated(
                AccountId: oldEvent.AccountId,
                CreationMethod: oldEvent.CreationMethod,
                EmailAddress: oldEvent.EmailAddress,
                PreferredCulture: oldEvent.PreferredCulture);
        }
    }
}
