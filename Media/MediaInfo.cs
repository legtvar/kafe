using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record MediaInfo(
    string FileExtension,
    string FormatName,
    string MimeType,
    long FileLength,
    TimeSpan Duration,
    ImmutableArray<VideoStreamInfo> VideoStreams,
    ImmutableArray<AudioStreamInfo> AudioStreams,
    ImmutableArray<SubtitleStreamInfo> SubtitleStreams,
    bool IsCorrupted = false
)
{
    public static MediaInfo Invalid { get; }
        = new(
            Const.InvalidFileExtension,
            Const.InvalidFormatName,
            Const.InvalidMimeType,
            -1,
            TimeSpan.Zero,
            ImmutableArray<VideoStreamInfo>.Empty,
            ImmutableArray<AudioStreamInfo>.Empty,
            ImmutableArray<SubtitleStreamInfo>.Empty,
            true);
}
