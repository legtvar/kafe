using System;
using System.Collections.Immutable;

namespace Kafe.Media;

public record MediaInfo(
    string FileExtension,
    string FormatName,
    string MimeType,
    long FileLength,
    TimeSpan Duration,
    double Bitrate,
    ImmutableArray<VideoStreamInfo> VideoStreams,
    ImmutableArray<AudioStreamInfo> AudioStreams,
    ImmutableArray<SubtitleStreamInfo> SubtitleStreams,
    bool IsCorrupted = false,
    string? Error = null
)
{
    public static MediaInfo Invalid { get; } = new(
        FileExtension: Const.InvalidFileExtension,
        FormatName: Const.InvalidFormatName,
        MimeType: Const.InvalidMimeType,
        FileLength: -1,
        Duration: TimeSpan.Zero,
        Bitrate: 0,
        VideoStreams: ImmutableArray<VideoStreamInfo>.Empty,
        AudioStreams: ImmutableArray<AudioStreamInfo>.Empty,
        SubtitleStreams: ImmutableArray<SubtitleStreamInfo>.Empty,
        IsCorrupted: true,
        Error: null);
}
