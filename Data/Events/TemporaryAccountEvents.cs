﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Events;

public record TemporaryAccountRefreshed(
    [Hrib] string AccountId,
    string? SecurityStamp
);
