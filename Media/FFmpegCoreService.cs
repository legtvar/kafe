using FFMpegCore;
using FFMpegCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Media;

public class FFmpegCoreService : IMediaService
{
    public FFmpegCoreService()
    {
        var path = FFmpeg.FindExecutable();
        if (path is null)
        {
            throw new InvalidOperationException("FFmpeg could not be found.");
        }

        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.GetDirectoryName(path)! });
    }

    public Task<bool> ConvertToPreset(Hrib hrib, VideoQualityPreset preset, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ImmutableArray<VideoQualityPreset> GetAvailablePresets(Hrib hrib)
    {
        throw new NotImplementedException();
    }

    public async Task<MediaInfo> GetInfo(string filePath, CancellationToken token = default)
    {
        try
        {
            var data = await FFProbe.AnalyseAsync(filePath, cancellationToken: token);

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
                .Select(s => new SubtitlesInfo(
                    Language: s.Language,
                    Codec: s.CodecName,
                    Bitrate: s.BitRate))
                .ToImmutableArray();

            return new MediaInfo(
                FileExtension: Path.GetExtension(filePath),
                FormatName: data.Format.FormatName,
                Duration: data.Duration,
                VideoStreams: videoInfos,
                AudioStreams: audioInfos,
                SubtitleStreams: subtitleInfos);
        }
        catch(FFMpegException)
        {
            return MediaInfo.Invalid;
        }
    }

    public Stream? Load(Hrib hrib, VideoQualityPreset preset = VideoQualityPreset.Original)
    {
        throw new NotImplementedException();
    }

    public Task Save(Hrib hrib, Stream data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
