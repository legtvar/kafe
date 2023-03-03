using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record AccountProjectAdded(
    Hrib AccountId,
    Hrib ProjectId
);

public record AccountProjectRemoved(
    Hrib AccountId,
    Hrib ProjectId
);
