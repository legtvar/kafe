using System;
using System.Collections.Immutable;
using System.Linq;
using Kafe.Data.Aggregates;
using Kafe.Media;

namespace Kafe.Api.Transfer;

public static class TransferMaps
{
    public static ProjectListDto ToProjectListDto(ProjectInfo data)
    {
        return new ProjectListDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            ReleaseDate: data.ReleaseDate);
    }

    public static ProjectDetailDto ToProjectDetailDto(ProjectInfo data)
    {
        return new ProjectDetailDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            ProjectGroupName: null,
            Genre: data.Genre,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            ReleaseDate: data.ReleaseDate,
            Crew: ImmutableArray<ProjectAuthorDto>.Empty,
            Cast: ImmutableArray<ProjectAuthorDto>.Empty,
            Artifacts: ImmutableArray<ArtifactDetailDto>.Empty
        );
    }

    public static AuthorListDto ToAuthorListDto(AuthorInfo data)
    {
        return new AuthorListDto(
            Id: data.Id,
            Name: data.Name);
    }

    public static AuthorDetailDto ToAuthorDetailDto(AuthorInfo data)
    {
        return new AuthorDetailDto(
            Id: data.Id,
            Name: data.Name,
            Bio: data.Bio,
            Uco: data.Uco,
            Email: data.Email,
            Phone: data.Phone);
    }

    public static PlaylistListDto ToPlaylistListDto(PlaylistInfo data)
    {
        return new PlaylistListDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility);
    }

    public static PlaylistDetailDto ToPlaylistDetailDto(PlaylistInfo data)
    {
        return new PlaylistDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            Videos: data.VideoIds);
    }

    public static ProjectGroupListDto ToProjectGroupListDto(ProjectGroupInfo data)
    {
        return new ProjectGroupListDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen);
    }

    public static ProjectGroupDetailDto ToProjectGroupDetailDto(ProjectGroupInfo data)
    {
        return new ProjectGroupDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen);
    }

    public static ArtifactDetailDto ToArtifactDetailDto(ArtifactDetail data)
    {
        return new ArtifactDetailDto(
            Id: data.Id,
            Name: data.Name,
            Shards: data.Shards.Select(ToShardListDto).ToImmutableArray(),
            ContainingProjectIds: data.ContainingProjectIds
        );
    }

    public static ShardListDto ToShardListDto(ArtifactShardInfo data)
    {
        return new ShardListDto(
            Id: data.ShardId,
            Kind: data.Kind,
            Variants: data.Variants);
    }

    public static VideoShardDetailDto ToVideoShardDetailDto(VideoShardInfo data)
    {
        return new VideoShardDetailDto(
            Id: data.Id,
            Kind: data.Kind,
            ArtifactId: data.ArtifactId,
            Variants: data.Variants.ToImmutableDictionary(v => v.Name, v => ToMediaInfoDto(v.Info)));
    }

    public static MediaDto ToMediaInfoDto(MediaInfo data)
    {
        return new MediaDto(
            FileExtension: data.FileExtension,
            MimeType: data.GetMimeType(),
            Duration: data.Duration,
            VideoStreams: data.VideoStreams.Select(ToVideoStreamDto).ToImmutableArray(),
            AudioStreams: data.AudioStreams.Select(ToAudioStreamDto).ToImmutableArray(),
            SubtitleStreams: data.SubtitleStreams.Select(ToSubtitleStreamDto).ToImmutableArray(),
            IsCorrupted: data.IsCorrupted);
    }

    public static VideoStreamDto ToVideoStreamDto(VideoInfo data)
    {
        return new VideoStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Width: data.Width,
            Height: data.Height,
            Framerate: data.Framerate);
    }

    public static AudioStreamDto ToAudioStreamDto(AudioInfo data)
    {
        return new AudioStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate,
            Channels: data.Channels,
            SampleRate: data.SampleRate);
    }

    public static SubtitleStreamDto ToSubtitleStreamDto(SubtitleInfo data)
    {
        return new SubtitleStreamDto(
            Codec: data.Codec,
            Bitrate: data.Bitrate);
    }

    public static ImageShardDetailDto ToImageShardDetailDto(ImageShardInfo data)
    {
        return new ImageShardDetailDto(
            Id: data.Id,
            Kind: data.Kind,
            ArtifactId: data.ArtifactId,
            Variants: data.Variants.ToImmutableDictionary(v => v.Name, v => ToImageDto(v.Info)));
    }

    public static ImageDto ToImageDto(ImageInfo data)
    {
        return new ImageDto(
            Width: data.Width,
            Height: data.Height,
            Format: data.Format);
    }

    public static ShardDetailBaseDto ToShardDetailDto(ShardInfoBase data)
    {
        return data switch
        {
            VideoShardInfo v => ToVideoShardDetailDto(v),
            ImageShardInfo i => ToImageShardDetailDto(i),
            _ => throw new NotSupportedException($"Shards of '{data.GetType()}' are not supported.")
        };
    }
}
