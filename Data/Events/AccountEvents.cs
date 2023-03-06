using Kafe.Data.Capabilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record AccountCapabilityAdded(
    Hrib AccountId,
    AccountCapability Capability
);

public record AccountCapabilityRemoved(
    Hrib AccountId,
    AccountCapability Capability
);
