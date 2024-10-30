using HotChocolate;
using HotChocolate.Data;
using Kafe.Api.Transfer;
using Kafe.Data.Aggregates;
using Marten;

namespace Kafe.Api;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public IExecutable<ProjectInfo> GetProjects([Service] IQuerySession querySession)
    {
        var queryable = querySession.Query<ProjectInfo>();
        return queryable.AsExecutable();
    }

    [UseFiltering]
    [UseSorting]
    public IExecutable<ProjectGroupInfo> GetProjectGroups([Service] IQuerySession querySession)
    {
        var queryable = querySession.Query<ProjectGroupInfo>();
        return queryable.AsExecutable();
    }

    [UseFiltering]
    [UseSorting]
    public IExecutable<PlaylistInfo> GetPlaylists([Service] IQuerySession querySession)
    {
        var queryable = querySession.Query<PlaylistInfo>();
        return queryable.AsExecutable();
    }
    
    [UseFiltering]
    [UseSorting]
    public IExecutable<ArtifactInfo> GetArtifacts([Service] IQuerySession querySession)
    {
        var queryable = querySession.Query<ArtifactInfo>();
        return queryable.AsExecutable();
    }
    
    [UseFiltering]
    [UseSorting]
    public IExecutable<VideoShardInfo> GetVideoShards([Service] IQuerySession querySession)
    {
        var queryable = querySession.Query<VideoShardInfo>();
        return queryable.AsExecutable();
    }
}
