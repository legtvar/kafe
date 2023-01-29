using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public class FFMpegCoreService : IMediaService
{
    public FFMpegCoreService()
    {
        var path = FFmpeg.FindExecutable();
        if (path is null)
        {
            throw new InvalidOperationException("FFmpeg could not be found.");
        }

        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.GetDirectoryName(path)! });
    }

    public async Task<MediaInfo> GetInfo(string filePath)
    {
        var data = await FFProbe.AnalyseAsync(filePath);

        var videoInfos = data.VideoStreams
            .Select(v => new VideoInfo(
                Codec: v.CodecName,
                Bitrate: v.BitRate,
                Width: v.Width,
                Height: v.Height,
                Framerate: v.FrameRate))
            .ToImmutableArray();

        var audioInfos = data.AudioStreams
            .Select(a => new AudioInfo(
                Codec: a.CodecName,
                Bitrate: a.BitRate,
                Channels: a.Channels,
                SampleRate: a.SampleRateHz))
            .ToImmutableArray();

        var subtitleInfos = data.SubtitleStreams
            .Select(s => new SubtitleInfo(
                Codec: s.CodecName,
                Bitrate: s.BitRate))
            .ToImmutableArray();

        return new MediaInfo(
            Path: filePath,
            Duration: data.Duration,
            VideoStreams: videoInfos,
            AudioStreams: audioInfos,
            SubtitleStreams: subtitleInfos);
    }
}
