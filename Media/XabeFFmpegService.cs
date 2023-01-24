﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Kafe.Media;

public class XabeFFmpegService : IMediaService
{
    public async Task<MediaInfo> GetInfo(string filePath)
    {
        var data = await FFmpeg.GetMediaInfo(filePath);

        var videoInfos = data.VideoStreams
            .Select(v => new VideoInfo(
                Codec: v.Codec,
                Bitrate: v.Bitrate,
                Width: v.Width,
                Height: v.Height,
                Framerate: v.Framerate))
            .ToImmutableArray();

        var audioInfos = data.AudioStreams
            .Select(a => new AudioInfo(
                Codec: a.Codec,
                Bitrate: a.Bitrate,
                Channels: a.Channels,
                SampleRate: a.SampleRate))
            .ToImmutableArray();

        var subtitleInfos = data.SubtitleStreams
            .Select(s => new SubtitleInfo(
                Codec: s.Codec,
                Bitrate: default))
            .ToImmutableArray();

        return new MediaInfo(
            Path: filePath,
            Duration: data.Duration,
            VideoStreams: videoInfos,
            AudioStreams: audioInfos,
            SubtitleStreams: subtitleInfos);
    }
}
