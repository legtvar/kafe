﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record AudioInfo(
    string Codec,
    long Bitrate,
    int Channels,
    int SampleRate);
