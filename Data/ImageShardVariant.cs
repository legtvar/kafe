﻿using Kafe.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data;

public record ImageShardVariant(
    string Name,
    ImageInfo Info
);
