using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record VideoInfo(
    int Width,
    int Height,
    double Framerate,
    long Bitrate);
