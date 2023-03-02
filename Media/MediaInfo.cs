using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record MediaInfo(
    string FileExtension,
    string FormatName,
    TimeSpan Duration,
    ImmutableArray<VideoInfo> VideoStreams,
    ImmutableArray<AudioInfo> AudioStreams,
    ImmutableArray<SubtitlesInfo> SubtitleStreams,
    bool IsCorrupted = false
)
{
    public static MediaInfo Invalid { get; }
        = new(
            Const.InvalidFileExtension,
            Const.InvalidFormatName,
            TimeSpan.Zero,
            ImmutableArray<VideoInfo>.Empty,
            ImmutableArray<AudioInfo>.Empty,
            ImmutableArray<SubtitlesInfo>.Empty,
            true);
}
