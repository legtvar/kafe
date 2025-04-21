using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record AudioStreamInfo(
    string Codec,
    long Bitrate,
    int Channels,
    int SampleRate
) : IMediaStreamInfo;
