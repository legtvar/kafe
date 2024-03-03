#pragma warning disable 0612,0618

using System;
using Marten.Services.Json.Transformations;

namespace Kafe.Data.Events
{
    [Obsolete]
    public record TemporaryAccountClosed(
        [Hrib] string AccountId
    );
}

namespace Kafe.Data.Events.Upcasts
{
    internal class TemporaryAccountClosedUpcaster : EventUpcaster<TemporaryAccountClosed, TemporaryAccountRefreshed>
    {
        protected override TemporaryAccountRefreshed Upcast(TemporaryAccountClosed oldEvent)
        {
            return new TemporaryAccountRefreshed(
                AccountId: oldEvent.AccountId,
                SecurityStamp: null);
        }
    }
}
