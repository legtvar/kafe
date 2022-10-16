namespace Kafe.Data;

public record ProjectCreated(
    CreationMethod CreationMethod,
    string ProjectGroupId
);
public record ProjectOwnerAdded(
    string OwnerId
);
public record ProjectOwnerRemoved(
    string OwnerId
);
public record ProjectAuthorAdded(
    string AuthorId,
    List<IndustryJobs>? IdustryJobs = null,
    List<string>? OtherJobs = null);
public record ProjectAuthorRemoved(
    string AuthorId
);
public record ProjectInfoChanged(
    string? Name = null,
    string? Description = null,
    string? EnglishName = null,
    string? EnglishDescription = null,
    ProjectVisibility? Visibility = null,
    DateTimeOffset? ReleaseDate = null,
    string? Link = null
);
public record ProjectVideoAdded(
    string VideoId
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
    string? Reason
);
public record ProjectPassedManualValidation;
public record ProjectFailedManualValidation(
    string? Reason
);
public record ProjectPassedDramaturgy;
public record ProjectFailedDramaturgy(
    string? Reason
);
public record ProjectValidationReset;
