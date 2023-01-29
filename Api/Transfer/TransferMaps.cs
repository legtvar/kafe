using System.Collections.Immutable;
using Kafe.Data.Aggregates;

namespace Kafe.Transfer;

public static class TransferMaps
{
    public static ProjectListDto ToProjectListDto(Project data)
    {
        return new ProjectListDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            ReleaseDate: data.ReleaseDate);
    }

    public static ProjectDetailDto ToProjectDetailDto(Project data)
    {
        return new ProjectDetailDto(
            Id: data.Id,
            ProjectGroupId: data.ProjectGroupId,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            Authors: data.Authors.Select(a => a.Id).ToImmutableArray(),
            ReleaseDate: data.ReleaseDate);
    }

    public static AuthorListDto ToAuthorListDto(Author data)
    {
        return new AuthorListDto(
            Id: data.Id,
            Name: data.Name);
    }

    public static AuthorDetailDto ToAuthorDetailDto(Author data)
    {
        return new AuthorDetailDto(
            Id: data.Id,
            Name: data.Name,
            Uco: data.Uco,
            Email: data.Email,
            Phone: data.Phone);
    }

    public static PlaylistListDto ToPlaylistListDto(Playlist data)
    {
        return new PlaylistListDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility);
    }

    public static PlaylistDetailDto ToPlaylistDetailDto(Playlist data)
    {
        return new PlaylistDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Visibility: data.Visibility,
            Videos: data.VideoIds);
    }
    
    public static ProjectGroupListDto ToProjectGroupListDto(ProjectGroup data)
    {
        return new ProjectGroupListDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen);
    }
    
    public static ProjectGroupDetailDto ToProjectGroupDetailDto(ProjectGroup data)
    {
        return new ProjectGroupDetailDto(
            Id: data.Id,
            Name: data.Name,
            Description: data.Description,
            Deadline: data.Deadline,
            IsOpen: data.IsOpen,
            ValidationRules: data.ValidationRules);
    }
}
