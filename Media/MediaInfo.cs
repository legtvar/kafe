using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Media;

public record MediaInfo(
    string Path,
    TimeSpan Duration,
    DateTimeOffset? CreationTime,
    ImmutableArray<VideoInfo> VideoStreams,
    ImmutableArray<AudioInfo> AudioStreams,
    ImmutableArray<SubtitleInfo> SubtitleStreams);

