using System;
using System.Collections.Immutable;
using Kafe.Media.Diagnostics;

namespace Kafe.Media;

public static class MediaConst
{
    public static readonly ImmutableDictionary<MediaStreamKind, Type> MediaStreamTypes
        = ImmutableDictionary.CreateRange<MediaStreamKind, Type>([
            new(MediaStreamKind.Video, typeof(VideoStreamInfo)),
            new(MediaStreamKind.Audio, typeof(AudioStreamInfo)),
            new(MediaStreamKind.Subtitles, typeof(SubtitleStreamInfo)),
        ]);

    public static object CreateMissingMediaStreamDiagnostic(
        Type mediaStreamType,
        Hrib shardId,
        LocalizedString shardName,
        string? variant,
        int streamIndex
    )
    {
        if (mediaStreamType == typeof(VideoStreamInfo))
        {
            return new MissingVideoStreamDiagnostic(shardName, shardId, variant, streamIndex);
        }

        if (mediaStreamType == typeof(AudioStreamInfo))
        {
            return new MissingAudioStreamDiagnostic(shardName, shardId, variant, streamIndex);
        }

        if (mediaStreamType == typeof(SubtitleStreamInfo))
        {
            return new MissingSubtitleStreamDiagnostic(shardName, shardId, variant, streamIndex);
        }

        throw new NotSupportedException($"Media stream type '{mediaStreamType}' is not supported.");
    }
}
