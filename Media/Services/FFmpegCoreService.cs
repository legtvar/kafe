using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Media.Services;

public class FFmpegCoreService : IMediaService
{
    private readonly ILogger<FFmpegCoreService> logger;

    public FFmpegCoreService(ILogger<FFmpegCoreService>? logger = null)
    {
        this.logger = logger ?? NullLogger<FFmpegCoreService>.Instance;
    }
    
    static FFmpegCoreService()
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
        catch (FFMpegException e)
        {
            return MediaInfo.Invalid with { Error = e.Message };
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
        catch (FFMpegException e)
        {
            return MediaInfo.Invalid with { Error = e.Message }; ;
        }
    }

    public async Task<MediaInfo> CreateVariant(
        string filePath,
        VideoQualityPreset preset,
        string? outputDir = null,
        bool overwrite = false,
        CancellationToken token = default)
    {
        var name = preset.ToFileName()
            ?? throw new ArgumentException($"Preset '{preset}' is not valid.");

        if (preset == VideoQualityPreset.Invalid || preset == VideoQualityPreset.Original)
        {
            throw new ArgumentException($"Video variant '{preset}' cannot be created.");
        }

        outputDir ??= Path.GetDirectoryName(filePath);
        if (outputDir is null || !Directory.Exists(outputDir))
        {
            throw new ArgumentException($"Output directory '{outputDir}' does not exist.");
        }

        var outputPath = Path.Combine(outputDir, $"{name}.webm");

        try
        {
            await FFMpegArguments
                .FromFileInput(filePath)
                .OutputToFile(outputPath, overwrite, o =>
                    o.WithVideoCodec("libvpx-vp9")
                    .WithAudioCodec("libopus")
                    .ForceFormat("webm")
                    .WithVideoFilters(f => f.Scale(-2, preset.ToHeight()))
                    .WithFastStart())
                .NotifyOnProgress(p => logger.LogDebug($"Progress {Path.GetFileName(filePath)} ({name}): '{p}'"))
                .NotifyOnOutput(p => logger.LogError(p))
                .CancellableThrough(token, 1_000)
                .ProcessAsynchronously(true);
        }
        catch (Exception)
        {
            File.Delete(outputPath);
            throw;
        }

        var info = await GetInfo(outputPath, token) with
        {
            // TODO: Figure out a way to handle the webm format correctly
            MimeType = "video/webm"
        };
        return info;
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
            Bitrate: data.Format.BitRate,
            VideoStreams: videoInfos,
            AudioStreams: audioInfos,
            SubtitleStreams: subtitleInfos);
    }
}
