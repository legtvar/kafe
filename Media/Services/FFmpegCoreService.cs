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

namespace Kafe.Media.Services;

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

    public async Task<MediaInfo> GetInfo(string filePath, CancellationToken token = default)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return MediaInfo.Invalid;
            }

            var data = await FFProbe.AnalyseAsync(filePath, cancellationToken: token);
            return GetMediaInfo(data) with
            {
                FileExtension = Path.GetExtension(filePath),
                FileLength = fileInfo.Length
            };
        }
        catch (FFMpegException)
        {
            return MediaInfo.Invalid;
        }
    }

    public async Task<MediaInfo> GetInfo(Stream stream, CancellationToken token = default)
    {
        try
        {
            var data = await FFProbe.AnalyseAsync(stream, cancellationToken: token);
            return GetMediaInfo(data) with
            {
                FileLength = stream.Length
            };
        }
        catch (FFMpegException)
        {
            return MediaInfo.Invalid;
        }
    }

    private MediaInfo GetMediaInfo(IMediaAnalysis data)
    {
        var videoInfos = data.VideoStreams
                .Select(v => new VideoStreamInfo(
                        Codec: v.CodecName,
                        Bitrate: v.BitRate,
                    Width: v.Width,
                    Height: v.Height,
                    Framerate: v.FrameRate))
                .ToImmutableArray();

        var audioInfos = data.AudioStreams
            .Select(a => new AudioStreamInfo(
                Codec: a.CodecName,
                Bitrate: a.BitRate,
                Channels: a.Channels,
                SampleRate: a.SampleRateHz))
            .ToImmutableArray();

        var subtitleInfos = data.SubtitleStreams
            .Select(s => new SubtitleStreamInfo(
                Language: s.Language,
                Codec: s.CodecName,
                Bitrate: s.BitRate))
            .ToImmutableArray();

        return new MediaInfo(
            FileExtension: FFmpegFormat.GetFileExtension(data.Format.FormatName) ?? Const.InvalidFileExtension,
            FormatName: data.Format.FormatName,
            MimeType: FFmpegFormat.GetMimeType(data.Format.FormatName) ?? Const.InvalidMimeType,
            FileLength: Const.InvalidFileLength,
            Duration: data.Duration,
            VideoStreams: videoInfos,
            AudioStreams: audioInfos,
            SubtitleStreams: subtitleInfos);
    }
}
