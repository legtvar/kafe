using System.Collections.Immutable;
using Kafe.Data;

namespace Kafe.Transfer;

public record ProjectListDto(
    string Id,
    string ProjectGroupId,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    DateTimeOffset ReleaseDate
    // string? Thumbnail
    // TODO: LocalizedString
    );

public record ProjectDetailDto(
    string Id,
    string ProjectGroupId,
    // string ProjectGroupName,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    ImmutableArray<string> Authors,
    DateTimeOffset ReleaseDate
    // Genre
    // Authors => Crew, Cast
    // Medias: Media[]
    );
    
// Media (Files: File[])
// File (FileType, Id, Validation Requirements, Validation Status)
// FileType: Image | Video | Subtitles | Other
