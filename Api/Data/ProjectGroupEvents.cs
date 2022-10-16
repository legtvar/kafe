namespace Kafe.Data;

public record ProjectGroupCreated(
    CreationMethod CreationMethod
);
public record ProjectGroupInfoChanged(
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    DateTimeOffset? Deadline = null);
public record ProjectGroupProjectOpened;
public record ProjectGroupProjectClosed;
public record ProjectGroupValidationRulesChanged(
    int? MinimumWidth,
    int? MinimumHeight,
    long? MaxFileSize,
    List<ContainerFormat>? AllowedContainerFormats,
    List<VideoCodec>? AllowedVideoCodecs,
    List<AudioCodec>? AllowedAudioCodecs,
    List<VideoFramerate>? AllowedVideoFramerates,
    List<SubtitleFormat>? AllowedSubtitleFormats
);
