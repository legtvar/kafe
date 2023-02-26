using System.Collections.Immutable;
using System.Linq;
using Kafe.Data.Aggregates;

namespace Kafe.Transfer;

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
            Artifacts: ImmutableArray<ProjectArtifactDto>.Empty
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

    public static ProjectArtifactDto ToProjectArtifactDto(ArtifactDetail data)
    {
        return new ProjectArtifactDto(
            Id: data.Id,
            Name: data.Name,
            Shards: data.Shards.Select(ToProjectArtifactShardDto).ToImmutableArray());
    }

    public static ProjectArtifactShardDto ToProjectArtifactShardDto(ArtifactShardInfo data)
    {
        return new ProjectArtifactShardDto(
            Id: data.ShardId,
            Kind: data.Kind,
            Variants: data.Variants);
    }
}
