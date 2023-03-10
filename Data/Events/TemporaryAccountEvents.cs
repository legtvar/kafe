using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record TemporaryAccountCreated(
    [Hrib] string AccountId,
    CreationMethod CreationMethod,
    string EmailAddress,
    string PreferredCulture
);

public record TemporaryAccountInfoChanged(
    string? PreferredCulture
);

public record TemporaryAccountRefreshed(
    [Hrib] string AccountId,
    string SecurityStamp
);

public record TemporaryAccountClosed(
    [Hrib] string AccountId
);
