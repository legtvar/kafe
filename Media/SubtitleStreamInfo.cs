﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record SubtitleStreamInfo(
    string? Language,
    string Codec,
    long Bitrate);
