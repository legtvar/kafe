namespace Kafe.Data;

public record ProjectGroupCreated;
public record ProjectGroupInfoChanged;
public record ProjectGroupProjectOpened;
public record ProjectGroupProjectClosed;
public record ProjectGroupValidationRulesChanged;

public record VideoConversionCreated;
public record VideoConversionCompleted;
public record VideoConversionFailed;

public record NotificationValidationCreated;
public record NotificationConversionFailureCreated;
public record NotificationDramaturgyCreated;
public record NotificationSent;

public record PlaylistCreated;
public record PlaylistInfoChanged;
public record PlaylistProjectAdded;
public record PlaylistProjectRemoved;
public record PlaylistProjectOrderChanged;

public record ProjectCreated;
public record ProjectOwnerAdded;
public record ProjectOwnerRemoved;
public record ProjectInfoChanged;
public record ProjectVideoAdded;
public record ProjectVideoRemoved;
public record ProjectLocked;
public record ProjectUnlocked;
public record ProjectPhotoAdded;
public record ProjectPhotoRemoved;
public record ProjectSubtitlesAdded;
public record ProjectSubtitlesRemoved;
public record ProjectPassedAutomaticValidation;
public record ProjectFailedAutomaticValidation;
public record ProjectPassedManualValidation;
public record ProjectFailedManualValidation;
public record ProjectPassedDramaturgy;
public record ProjectFailedDramaturgy;
public record ProjectValidationReset;
