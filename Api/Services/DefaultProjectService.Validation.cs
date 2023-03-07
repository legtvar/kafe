using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public partial class DefaultProjectService : IProjectService
{
    public const int NameLengthLimit = 32;
    public const int DescriptionMinLength = 50;
    public const int DescriptionMaxLength = 200;
    public const int GenreLengthLimit = 32;
    public static readonly TimeSpan FilmMinLength = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan FilmMaxLength = TimeSpan.FromMinutes(8);
    public static readonly TimeSpan VideoAnnotationMinLength = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan VideoAnnotationMaxLength = TimeSpan.FromSeconds(30);
    public static long FilmMaxFileLength = 2 << 30; // 2 GiB
    public static string FilmMaxFileLengthDescription = "2 GiB";

    public static long VideoAnnotationMaxFileLength = 512 << 20; // 512 MiB
    public static string VideoAnnotationMaxFileLengthDescription = "512 GiB";

    public const string InfoStage = "info";
    public const string FileStage = "file";
    public const string TechReviewStage = "tech-review";
    public const string VisualReviewStage = "visual-review";
    public const string DramaturgyReviewStage = "dramaturgy-review";

    public static readonly ProjectDiagnosticDto InvalidName = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project must have a name that is at most {NameLengthLimit} characters long."),
            (Const.CzechCulture, $"Projekt musí mít název o nejvýše {NameLengthLimit} znacích.")
        )
    );

    public static readonly ProjectDiagnosticDto InvalidDescription = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project must have a description that is " +
                $"between {DescriptionMinLength} and {DescriptionMaxLength} characters long."),
            (Const.CzechCulture, $"Projekt musí mít anotaci o " +
                $"délce {DescriptionMinLength} až {DescriptionMaxLength} znaků.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingGenre = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project is missing a genre."),
            (Const.CzechCulture, $"Projektu chybí žánr.")
        )
    );

    public static readonly ProjectDiagnosticDto GenreTooLong = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project genre must be at most {GenreLengthLimit} characters long."),
            (Const.CzechCulture, $"Žánr projektu musí mít nejvýše {DescriptionMaxLength} znaků.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingTechReview = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: TechReviewStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival technician."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalový technik.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingVisualReview = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: VisualReviewStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival graphic designer."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalový grafik.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingDramaturgyReview = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: DramaturgyReviewStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival jury."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalová porota.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingCrew = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: InfoStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "At least one crew member is required."),
            (Const.CzechCulture, "Je vyžadován alespoň jeden člen štábu.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingFilm = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film is required."),
            (Const.CzechCulture, "Film je povinnou součástí přihlášky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyFilms = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one film is permitted."),
            (Const.CzechCulture, "Součástí přihlášky může být pouze jeden film.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingVideoAnnotationVideo = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation is missing the video file."),
            (Const.CzechCulture, "Videoanotaci chybí soubor s videem.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyVideoAnnotations = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one video-annotation is permitted."),
            (Const.CzechCulture, "Součástí přihlášky může být pouze jedna videoanotace.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingFilmSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "English subtitles for the film file are required."),
            (Const.CzechCulture, "Anglické titulky pro film jsou povinné.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingVideoAnnotationSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation is missing English subtitles."),
            (Const.CzechCulture, "Videoanotaci chybí soubor s anglickými titulky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyFilmSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one file with English subtitles for the film is permitted."),
            (Const.CzechCulture, "Pouze jedny anglické titulky k filmu mohou být součástí přihlášky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyVideoAnnotationSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one file with English subtitles for the video-annotation is permitted."),
            (Const.CzechCulture, "Pouze jedny anglické titulky k videoanotaci mohou být součástí přihlášky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooFewCoverPhotos = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "At least one cover photo is required."),
            (Const.CzechCulture, "Alespoň jedna titulní fotka je vyžadována.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyCoverPhotos = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Too many cover photos have been provided."),
            (Const.CzechCulture, "Přihláška obsahuje příliš mnoho titulních fotek.")
        )
    );

    public static readonly ProjectDiagnosticDto FilmTooShort = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film is too short. Minimum length is '{FilmMinLength:c}'."),
            (Const.CzechCulture, $"Film je příliš krátký. Minimální délka je '{FilmMinLength:c}'.")
        )
    );

    public static readonly ProjectDiagnosticDto FilmTooLong = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film is too long. Maximum length is '{FilmMaxLength:c}'."),
            (Const.CzechCulture, $"Film je příliš krátký. Maximální délka je '{FilmMaxLength:c}'.")
        )
    );

    public static readonly ProjectDiagnosticDto VideoAnnotationTooShort = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation is too short. Minimum length is '{FilmMinLength:c}'."),
            (Const.CzechCulture, $"Videoanotace je příliš krátká. Minimální délka je '{FilmMinLength:c}'.")
        )
    );

    public static readonly ProjectDiagnosticDto VideoAnnotationTooLong = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation is too long. Maximum length is '{FilmMaxLength:c}'."),
            (Const.CzechCulture, $"Videoanotace je příliš krátká. Maximální délka je '{FilmMaxLength:c}'.")
        )
    );

    public static readonly ProjectDiagnosticDto FilmHasZeroFileLength = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file has zero file length."),
            (Const.CzechCulture, "Soubor s filmem má nulovou velikost.")
        )
    );

    public static readonly ProjectDiagnosticDto FilmIsTooLarge = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The film file is larger than {FilmMaxFileLengthDescription}."),
            (Const.CzechCulture, $"Soubor s filmem je větší než {FilmMaxFileLengthDescription}.")
        )
    );

    public static readonly ProjectDiagnosticDto VideoAnnotationHasZeroFileLength = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file has zero file length."),
            (Const.CzechCulture, "Soubor s videoanotací má nulovou velikost.")
        )
    );

    public static readonly ProjectDiagnosticDto VideoAnnotationIsTooLarge = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The video-annotation file is larger than {VideoAnnotationMaxFileLengthDescription}."),
            (Const.CzechCulture, $"Soubor s videoanotací je větší než {VideoAnnotationMaxFileLengthDescription}.")
        )
    );

    public static readonly ProjectDiagnosticDto FilmCorrupted = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film file is corrupted."),
            (Const.CzechCulture, "Soubor s filmem je poškozený.")
        )
    );

    public static readonly ProjectDiagnosticDto VideoAnnotationCorrupted = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        ValidationStage: FileStage,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation file is corrupted."),
            (Const.CzechCulture, "Soubor s videoanotací je poškozený.")
        )
    );


    // TODO: Blueprints instead of these hard-coded validation settings for FFFI MU 2023.
    public async Task<ProjectValidationDto> Validate(Hrib id, CancellationToken token = default)
    {
        var project = await db.LoadAsync<ProjectInfo>(id, token);
        if (project is null)
        {
            throw new IndexOutOfRangeException();
        }

        if (!userProvider.CanRead(project))
        {
            throw new UnauthorizedAccessException();
        }

        var diagnostics = ImmutableArray.CreateBuilder<ProjectDiagnosticDto>();

        if (project.Name.Values.Any(n => string.IsNullOrWhiteSpace(n) || n.Length > NameLengthLimit))
        {
            diagnostics.Add(InvalidName);
        }

        if (project.Description is null
            || project.Description.Values.Any(n => string.IsNullOrWhiteSpace(n)
            || n.Length < DescriptionMinLength
            || n.Length > DescriptionMaxLength))
        {
            diagnostics.Add(InvalidDescription);
        }

        if (project.Genre is null
            || project.Genre.Values.Any(g => string.IsNullOrWhiteSpace(g)))
        {
            diagnostics.Add(MissingGenre);
        }

        if (project.Genre is not null
            && project.Genre.Values.Any(g => g.Length > GenreLengthLimit))
        {
            diagnostics.Add(GenreTooLong);
        }

        if (!project.Reviews.Any(r => r.ReviewerRole == Const.TechReviewer && r.Kind == ReviewKind.Accepted))
        {
            diagnostics.Add(MissingTechReview);
        }

        if (!project.Reviews.Any(r => r.ReviewerRole == Const.VisualReviewer && r.Kind == ReviewKind.Accepted))
        {
            diagnostics.Add(MissingVisualReview);
        }

        if (!project.Reviews.Any(r => r.ReviewerRole == Const.VisualReviewer && r.Kind == ReviewKind.Accepted))
        {
            diagnostics.Add(MissingDramaturgyReview);
        }

        if (project.Authors.Count(a => a.Kind == ProjectAuthorKind.Crew) < 1)
        {
            diagnostics.Add(MissingCrew);
        }

        var artifactInfos = await db.LoadManyAsync<ArtifactDetail>(token, project.Artifacts.Select(a => a.Id));
        var artifacts = project.Artifacts
            .Join(artifactInfos, a => a.Id, i => i.Id, (a, i) => (projectArtifact: a, info: i))
            .ToImmutableArray();

        var filmArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.FilmBlueprintSlot)
            .ToImmutableArray();
        if (filmArtifacts.Length == 0)
        {
            diagnostics.Add(MissingFilm);
        }
        else if (filmArtifacts.Length > 1)
        {
            diagnostics.Add(TooManyFilms);
        }
        else
        {
            var videoShards = filmArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Video)
                .ToImmutableArray();
            if (videoShards.Length == 0)
            {
                diagnostics.Add(MissingFilm);
            }
            else if (videoShards.Length > 1)
            {
                diagnostics.Add(TooManyFilms);
            }
            else
            {
                var videoShard = await db.LoadAsync<VideoShardInfo>(videoShards[0].ShardId, token);
                if (videoShard is null)
                {
                    diagnostics.Add(MissingFilm);
                }
                else
                {
                    diagnostics.AddRange(ValidateVideo(
                        video: videoShard,
                        maxFileLength: FilmMaxFileLength,
                        minLength: FilmMinLength,
                        maxLength: FilmMaxLength,
                        corruptedError: FilmCorrupted,
                        zeroFileLengthError: FilmHasZeroFileLength,
                        tooLargeError: FilmHasZeroFileLength,
                        tooShortError: FilmTooShort,
                        tooLongError: FilmTooLong
                    ));
                }
            }

            var subtitleShards = filmArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Subtitles)
                .ToImmutableArray();
            if (subtitleShards.Length == 0)
            {
                diagnostics.Add(MissingFilmSubtitles);
            }
            else if (subtitleShards.Length > 1)
            {
                diagnostics.Add(TooManyFilmSubtitles);
            }
        }

        var videoAnnotationArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.VideoAnnotationBlueprintSlot)
            .ToImmutableArray();
        if (videoAnnotationArtifacts.Length > 1)
        {
            diagnostics.Add(TooManyVideoAnnotations);
        }
        else if (videoAnnotationArtifacts.Length == 1)
        {
            var videoShards = videoAnnotationArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Video)
                .ToImmutableArray();
            if (videoShards.Length == 0)
            {
                diagnostics.Add(MissingVideoAnnotationVideo);
            }
            else if (videoShards.Length > 1)
            {
                diagnostics.Add(TooManyVideoAnnotations);
            }
            else
            {
                var videoShard = await db.LoadAsync<VideoShardInfo>(videoShards[0].ShardId, token);
                if (videoShard is null)
                {
                    diagnostics.Add(MissingFilm);
                }
                else
                {
                    diagnostics.AddRange(ValidateVideo(
                        video: videoShard,
                        maxFileLength: VideoAnnotationMaxFileLength,
                        minLength: VideoAnnotationMinLength,
                        maxLength: VideoAnnotationMaxLength,
                        corruptedError: VideoAnnotationCorrupted,
                        zeroFileLengthError: VideoAnnotationHasZeroFileLength,
                        tooLargeError: VideoAnnotationHasZeroFileLength,
                        tooShortError: VideoAnnotationTooShort,
                        tooLongError: VideoAnnotationTooLong
                    ));
                }
            }

            var subtitleShards = videoAnnotationArtifacts.Single().info.Shards.Where(s => s.Kind == ShardKind.Subtitles)
                .ToImmutableArray();
            if (subtitleShards.Length == 0)
            {
                diagnostics.Add(MissingVideoAnnotationSubtitles);
            }
            else if (subtitleShards.Length > 1)
            {
                diagnostics.Add(TooManyVideoAnnotationSubtitles);
            }
        }

        var coverPhotoArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.CoverPhotoBlueprintSlot)
            .ToImmutableArray();
        if (coverPhotoArtifacts.Length < Const.CoverPhotoMinCount)
        {
            diagnostics.Add(TooFewCoverPhotos);
        }
        else if (coverPhotoArtifacts.Length > 5)
        {
            diagnostics.Add(TooManyCoverPhotos);
        }

        return new(
            ProjectId: id,
            ValidatedOn: DateTimeOffset.UtcNow,
            Diagnostics: diagnostics.ToImmutable()
        );
    }

    public Task AddReview(Hrib projectId, ProjectReviewDto dto)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<ProjectDiagnosticDto> ValidateVideo(
        VideoShardInfo video,
        long maxFileLength,
        TimeSpan minLength,
        TimeSpan maxLength,
        ProjectDiagnosticDto corruptedError,
        ProjectDiagnosticDto zeroFileLengthError,
        ProjectDiagnosticDto tooLargeError,
        ProjectDiagnosticDto tooShortError,
        ProjectDiagnosticDto tooLongError)
    {
        if (!video.Variants.Keys.Contains(Const.OriginalShardVariant))
        {
            yield return MissingFilm;
            yield break;
        }

        var originalVariant = video.Variants[Const.OriginalShardVariant];
        if (originalVariant.IsCorrupted)
        {
            yield return corruptedError;
            yield break;
        }

        if (originalVariant.FileLength == 0)
        {
            yield return zeroFileLengthError;
        }
        else if (originalVariant.FileLength > maxFileLength)
        {
            yield return tooLargeError;
        }

        if (originalVariant.Duration < minLength)
        {
            yield return tooShortError;
        }
        else if (originalVariant.Duration > maxLength)
        {
            yield return tooLongError;
        }
    }
}
