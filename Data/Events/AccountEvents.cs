using Kafe.Data.Capabilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record AccountPermissionSet(
    [Hrib] string AccountId,
    string EntityId,
    Permission Permission
);

public record AccountPermissionUnset(
    [Hrib] string AccountId,
    string EntityId
);
