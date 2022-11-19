using System.Collections.Immutable;

namespace Kafe.Data;

public record ValidationRules(
    int? MinimumWidth,
    int? MinimumHeight,
    long? MaxFileSize,
    ImmutableArray<ContainerFormat> AllowedContainerFormats,
    ImmutableArray<VideoCodec> AllowedVideoCodecs,
    ImmutableArray<AudioCodec> AllowedAudioCodecs,
    ImmutableArray<VideoFramerate> AllowedVideoFramerates,
    ImmutableArray<SubtitleFormat> AllowedSubtitleFormats
);
