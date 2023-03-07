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
    public const int DescriptionLengthLimit = 270;
    public const int GenreLengthLimit = 32;

    public static readonly ProjectDiagnosticDto InvalidName = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project must have a name that is at most {NameLengthLimit} characters long."),
            (Const.CzechCulture, $"Projekt musí mít název o nejvýše {NameLengthLimit} znacích.")
        )
    );

    public static readonly ProjectDiagnosticDto InvalidDescription = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project must have a description that is at most {DescriptionLengthLimit} characters long."),
            (Const.CzechCulture, $"Projekt musí mít anotaci o nejvýše {DescriptionLengthLimit} znacích.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingGenre = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project is missing a genre."),
            (Const.CzechCulture, $"Projektu chybí žánr.")
        )
    );

    public static readonly ProjectDiagnosticDto GenreTooLong = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, $"The project genre must be at most {GenreLengthLimit} characters long."),
            (Const.CzechCulture, $"Žánr projektu musí mít nejvýše {DescriptionLengthLimit} znaků.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingTechReview = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival technician."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalový technik.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingVisualReview = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival graphic designer."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalový grafik.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingDramaturgyReview = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "This project is yet to be accepted by a festival jury."),
            (Const.CzechCulture, "Tento project ještě musí přijmout festivalová porota.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingCrew = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "At least one crew member is required."),
            (Const.CzechCulture, "Je vyžadován alespoň jeden člen štábu.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingFilm = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The film is required."),
            (Const.CzechCulture, "Film je povinnou součástí přihlášky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyFilms = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one film is permitted."),
            (Const.CzechCulture, "Součástí přihlášky může být pouze jeden film.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingVideoAnnotationVideo = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation is missing the video file."),
            (Const.CzechCulture, "Videoanotaci chybí soubor s videem.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyVideoAnnotations = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one video-annotation is permitted."),
            (Const.CzechCulture, "Součástí přihlášky může být pouze jedna videoanotace.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingFilmSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "English subtitles for the film file are required."),
            (Const.CzechCulture, "Anglické titulky pro film jsou povinné.")
        )
    );

    public static readonly ProjectDiagnosticDto MissingVideoAnnotationSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "The video-annotation is missing English subtitles."),
            (Const.CzechCulture, "Videoanotaci chybí soubor s anglickými titulky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyFilmSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one file with English subtitles for the film is permitted."),
            (Const.CzechCulture, "Pouze jedny anglické titulky k filmu mohou být součástí přihlášky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyVideoAnnotationSubtitles = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Only one file with English subtitles for the video-annotation is permitted."),
            (Const.CzechCulture, "Pouze jedny anglické titulky k videoanotaci mohou být součástí přihlášky.")
        )
    );

    public static readonly ProjectDiagnosticDto TooFewCoverPhotos = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "At least one cover photo is required."),
            (Const.CzechCulture, "Alespoň jedna titulní fotka je vyžadována.")
        )
    );

    public static readonly ProjectDiagnosticDto TooManyCoverPhotos = new ProjectDiagnosticDto(
        Kind: DiagnosticKind.Error,
        Message: LocalizedString.Create(
            (Const.InvariantCulture, "Too many cover photos have been provided."),
            (Const.CzechCulture, "Přihláška obsahuje příliš mnoho titulních fotek.")
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
            || project.Description.Values.Any(n => string.IsNullOrWhiteSpace(n) || n.Length > DescriptionLengthLimit))
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

    public async Task AddReview(Hrib projectId, ProjectReviewDto dto)
    {
        throw new NotImplementedException();
    }
}
