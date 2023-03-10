using Kafe.Data.Capabilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record AccountCapabilityAdded(
    [Hrib] string AccountId,
    [AccountCapability] string Capability
);

public record AccountCapabilityRemoved(
    [Hrib] string AccountId,
    [AccountCapability] string Capability
);
