using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public class FFMpegCoreService : IMediaService
{
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
