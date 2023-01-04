using System.Collections.Immutable;

namespace Kafe.Data.Events;

public record ProjectCreated(
    CreationMethod CreationMethod,
    string ProjectGroupId,
    LocalizedString Name,
    Visibility Visibility
);
public record ProjectAuthorAdded(
    string AuthorId,
    ImmutableArray<string>? Jobs = null
);
public record ProjectAuthorRemoved(
    string AuthorId
);
public record ProjectInfoChanged(
    LocalizedString? Name = null,
    LocalizedString? Description = null,
    Visibility? Visibility = null,
    DateTimeOffset? ReleaseDate = null
);
public record ProjectVideoAdded(
    string VideoId,
    LocalizedString Name,
    VideoKind Kind = default
);
public record ProjectVideoRemoved(
    string VideoId
);
public record ProjectLocked;
public record ProjectUnlocked;
public record ProjectPhotoAdded(
    string PhotoId
);
public record ProjectPhotoRemoved(
    string PhotoId
);
public record ProjectSubtitlesAdded(
    string SubtitlesId
);
public record ProjectSubtitlesRemoved(
    string SubtitlesId
);
public record ProjectPassedAutomaticValidation;
public record ProjectFailedAutomaticValidation(
    LocalizedString Reason
);
public record ProjectPassedManualValidation;
public record ProjectFailedManualValidation(
    LocalizedString Reason
);
public record ProjectPassedDramaturgy;
public record ProjectFailedDramaturgy(
    LocalizedString Reason
);
public record ProjectValidationReset;
