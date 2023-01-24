using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Kafe.Media;

public class XabeFFmpegService : IMediaService
{
    public async Task<MediaInfo> GetInfo(string filePath)
    {
        var test = await FFmpeg.GetMediaInfo(filePath);
        return new MediaInfo(default, default, default, default, default, default);
    }
}
