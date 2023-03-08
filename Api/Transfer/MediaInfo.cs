using System;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record MediaDto(
    string FileExtension,
    string MimeType,
    long FileLength,
    TimeSpan Duration,
    ImmutableArray<VideoStreamDto> VideoStreams,
    ImmutableArray<AudioStreamDto> AudioStreams,
    ImmutableArray<SubtitleStreamDto> SubtitleStreams,
    bool IsCorrupted);

public record VideoStreamDto(
    string Codec,
    long Bitrate,
    int Width,
    int Height,
    double Framerate
);

public record AudioStreamDto(
    string Codec,
    long Bitrate,
    int Channels,
    int SampleRate
);

public record SubtitleStreamDto(
    string Codec,
    long Bitrate
);

public record ImageDto(
    string FileExtension,
    string MimeType,
    int Width,
    int Height,
    bool IsCorrupted
);