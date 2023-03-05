using Kafe.Data.Capabilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record AccountCapabilityAdded(
    IAccountCapability Capability
);

public record AccountCapabilityRemoved(
    IAccountCapability Capability
);
