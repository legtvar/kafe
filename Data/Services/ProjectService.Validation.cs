using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public partial class ProjectService
{
    // TODO: Blueprints instead of these hard-coded validation settings for FFFI MU 2023.
    public async Task<ProjectReport> Validate(Hrib id, CancellationToken token = default)
    {
        var project = (await db.LoadAsync<ProjectInfo>(id.ToString(), token)).Unwrap();
        if (project is null)
        {
            throw new IndexOutOfRangeException();
        }

        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        if (project.Name.Values.Any(n => string.IsNullOrWhiteSpace(n) || n.Length > NameMaxLength))
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
            && project.Genre.Values.Any(g => g.Length > GenreMaxLength))
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
                        tooLargeError: FilmIsTooLarge,
                        tooShortError: FilmTooShort,
                        tooLongError: FilmTooLong,
                        streamMismatchError: FilmInvalidStreamCount,
                        unsupportedContainerError: FilmUnsupportedContainerFormat,
                        bitrateTooLowError: FilmUnsupportedBitrate,
                        bitrateTooHighError: FilmUnsupportedBitrate,
                        unsupportedVideoCodecError: FilmUnsupportedVideoCodec,
                        unsupportedAudioCodecError: FilmUnsupportedAudioCodec,
                        mp3BitrateTooLowError: FilmMp3BitrateTooLow,
                        unsupportedFramerateError: FilmUnsupportedFramerate,
                        wrongResolutionError: FilmWrongResolution
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
            else
            {
                var subtitleShard = await db.LoadAsync<SubtitlesShardInfo>(subtitleShards[0].ShardId, token);
                if (subtitleShard is null)
                {
                    diagnostics.Add(MissingFilmSubtitles);
                }
                else
                {
                    diagnostics.AddRange(ValidateSubtitles(subtitleShard));
                }
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
                        tooLargeError: VideoAnnotationIsTooLarge,
                        tooShortError: VideoAnnotationTooShort,
                        tooLongError: VideoAnnotationTooLong,
                        streamMismatchError: VideoAnnotationInvalidStreamCount,
                        unsupportedContainerError: VideoAnnotationUnsupportedContainerFormat,
                        bitrateTooLowError: VideoAnnotationUnsupportedBitrate,
                        bitrateTooHighError: VideoAnnotationUnsupportedBitrate,
                        unsupportedVideoCodecError: VideoAnnotationUnsupportedVideoCodec,
                        unsupportedAudioCodecError: VideoAnnotationUnsupportedAudioCodec,
                        mp3BitrateTooLowError: VideoAnnotationMp3BitrateTooLow,
                        unsupportedFramerateError: VideoAnnotationUnsupportedFramerate,
                        wrongResolutionError: VideoAnnotationWrongResolution
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
            else
            {
                var subtitleShard = await db.LoadAsync<SubtitlesShardInfo>(subtitleShards[0].ShardId, token);
                if (subtitleShard is null)
                {
                    diagnostics.Add(MissingVideoAnnotationSubtitles);
                }
                else
                {
                    diagnostics.AddRange(ValidateSubtitles(subtitleShard));
                }
            }
        } 

        var coverPhotoArtifacts = artifacts.Where(a => a.projectArtifact.BlueprintSlot == Const.CoverPhotoBlueprintSlot)
            .ToImmutableArray();
        if (coverPhotoArtifacts.Length < Const.CoverPhotoMinCount)
        {
            diagnostics.Add(TooFewCoverPhotos);
        }
        else if (coverPhotoArtifacts.Length > Const.CoverPhotoMaxCount)
        {
            diagnostics.Add(TooManyCoverPhotos);
        }
        else
        {
            foreach (var coverPhotoArtifact in coverPhotoArtifacts)
            {
                if (coverPhotoArtifact.info.Shards.Length != 1)
                {
                    diagnostics.Add(MissingCoverPhotoFile);
                    continue;
                }

                var imageShard = await db.LoadAsync<ImageShardInfo>(
                    coverPhotoArtifact.info.Shards.Single().ShardId,
                    token);
                if (imageShard is null)
                {
                    diagnostics.Add(MissingCoverPhotoFile);
                    continue;
                }
                diagnostics.AddRange(ValidateImage(imageShard));
            }
        }

        return new(
            ProjectId: id,
            ValidatedOn: DateTimeOffset.UtcNow,
            Diagnostics: diagnostics.ToImmutable()
        );
    }

    public async Task<Err<bool>> AddReview(
        Hrib projectId,
        ReviewKind kind,
        string reviewerRole,
        LocalizedString? comment,
        CancellationToken token = default)
    {
        var project = await Load(projectId, token);
        if (project is null)
        {
            return new Kafe.Diagnostic($"Project {projectId} could not be found.");
        }

        if (kind == ReviewKind.NotReviewed)
        {
            return new Kafe.Diagnostic($"Review cannot be '{kind}'. It must be either accepting or rejecting.");
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<ProjectInfo>(projectId.ToString(), token);
        var reviewAdded = new ProjectReviewAdded(projectId.ToString(), kind, reviewerRole, comment);
        eventStream.AppendOne(reviewAdded);
        await db.SaveChangesAsync(token);
        return true;
    }
}
