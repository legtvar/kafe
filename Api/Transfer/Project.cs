using System;
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
    LocalizedString ProjectGroupName,
    LocalizedString? Genre,
    LocalizedString Name,
    LocalizedString? Description,
    Visibility Visibility,
    ImmutableArray<string> Authors,
    DateTimeOffset ReleaseDate,
    ImmutableArray<ProjectAuthorDto> Crew,
    ImmutableArray<ProjectAuthorDto> Cast

    // Authors => Crew, Cast
    // Medias: Media[]
    );

public record ProjectAuthorDto(
    string Id,
    string Name,
    ImmutableArray<string> Jobs);


// Media (Files: File[])
// File (FileType, Id, Validation Requirements, Validation Status)
// FileType: Image | Video | Subtitles | Other
