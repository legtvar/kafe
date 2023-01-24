﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record VideoInfo(
    string Codec,
    long Bitrate,
    int Width,
    int Height,
    double Framerate);
