using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Media;
using Marten;

namespace Kafe.Migrator;

public sealed class KafeClient : IAsyncDisposable
{
    private readonly IDocumentStore martenStore;
    private readonly ILogger<KafeClient> logger;
    private static IDocumentSession session = null!;

    public KafeClient(
        IDocumentStore martenStore,
        ILogger<KafeClient> logger)
    {
        this.martenStore = martenStore;
        this.logger = logger;
    }

    public async Task Initialize()
    {
        await martenStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        session = martenStore.DirtyTrackedSession();
    }

    public async ValueTask DisposeAsync()
    {
        martenStore.Dispose();
        await session.DisposeAsync();
    }

    public async Task<int> CountAuthors()
    {
        return await session.Query<AuthorInfo>().CountAsync();
    }

    public async Task<int> CountProjectGroups()
    {
        return await session.Query<ProjectGroupInfo>().CountAsync();
    }

    public async Task<int> CountProjects()
    {
        return await session.Query<ProjectInfo>().CountAsync();
    }

    public async Task<int> CountPlaylists()
    {
        return await session.Query<PlaylistInfo>().CountAsync();
    }

    public async Task<int> CountArtifacts()
    {
        return await session.Query<ArtifactInfo>().CountAsync();
    }

    public async Task<AuthorInfo> CreateAuthor(string? name, string? uco, string? email, string? phone, Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();
        name ??= string.Format(Fallback.AuthorName, hrib);
        var created = new AuthorCreated(
            AuthorId: hrib.Value,
            CreationMethod: CreationMethod.Migrator,
            Name: name);
        LogEvent(hrib, created);

        var infoChanged = new AuthorInfoChanged(
            AuthorId: hrib.Value,
            Uco: uco,
            Email: email,
            Phone: phone);
        LogEvent(hrib, infoChanged);

        session.Events.StartStream<AuthorInfo>(hrib, created, infoChanged);
        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<AuthorInfo>(hrib.Value))!;
    }

    public async Task<AuthorInfo> GetOrAddAuthor(
        Hrib? hrib,
        string? name,
        string? uco,
        string? email,
        string? phone)
    {
        AuthorInfo? author = null;
        if (hrib is not null )
        {
            author = await session.Events.AggregateStreamAsync<AuthorInfo>(hrib.Value);
        }
        else if (name is not null)
        {
            author = await session.Query<AuthorInfo>().Where(a => a.Name == name).FirstOrDefaultAsync();
        }
        else if (uco is not null)
        {
            author = await session.Query<AuthorInfo>().Where(a => a.Uco == uco).FirstOrDefaultAsync();
        }
        else if (email is not null)
        {
            author = await session.Query<AuthorInfo>().Where(a => a.Email == email).FirstOrDefaultAsync();
        }
        else if (phone is not null)
        {
            author = await session.Query<AuthorInfo>().Where(a => a.Phone == phone).FirstOrDefaultAsync();
        }

        if (author is null)
        {
            author = await CreateAuthor(name, uco, email, phone);
        }
        else
        {
            var infoChanged = new AuthorInfoChanged(
                AuthorId: author.Id,
                Name: name,
                Uco: uco,
                Email: email,
                Phone: phone);
            if (name is not null || uco is not null || email is not null || phone is not null)
            {
                session.Events.Append(author.Id, infoChanged);
                LogEvent(author.Id, infoChanged);
            }
        }
        await session.SaveChangesAsync();
        return author!;
    }

    public async Task<(ArtifactInfo, VideoShardInfo)> CreateVideoArtifact(
        string name,
        MediaInfo originalVariant,
        DateTimeOffset addedOn,
        Hrib? projectId = null,
        Hrib? artifactId = null,
        Hrib? shardId = null)
    {
        artifactId ??= Hrib.Create();
        var artifactCreated = new ArtifactCreated(
            ArtifactId: artifactId.Value,
            CreationMethod: CreationMethod.Migrator,
            Name: (LocalizedString)name,
            AddedOn: addedOn);
        session.Events.StartStream<ArtifactInfo>(artifactId, artifactCreated);
        LogEvent(artifactId, artifactCreated);

        shardId ??= Hrib.Create();
        var shardCreated = new VideoShardCreated(
            ShardId: shardId.Value,
            CreationMethod: CreationMethod.Migrator,
            ArtifactId: artifactId.Value,
            OriginalVariantInfo: originalVariant);
        session.Events.StartStream<VideoShardInfo>(shardId, shardCreated);
        LogEvent(shardId, shardCreated);

        if (projectId is not null)
        {
            await AddArtifactToProject(artifactId, projectId);
        }

        await session.SaveChangesAsync();
        return ((await session.Events.AggregateStreamAsync<ArtifactInfo>(artifactId.Value))!,
            (await session.Events.AggregateStreamAsync<VideoShardInfo>(shardId.Value))!);
    }

    public async Task AddArtifactToProject(Hrib artifactId, Hrib projectId)
    {
        var artifactAdded = new ProjectArtifactAdded(projectId.Value, artifactId.Value, null);
        LogEvent(projectId, artifactAdded);
        session.Events.Append(projectId.Value, artifactAdded);

        await session.SaveChangesAsync();
    }

    public async Task<PlaylistInfo> CreatePlaylist(
        string name,
        string? description,
        IEnumerable<Hrib>? videos,
        Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();

        var created = new PlaylistCreated(
            PlaylistId: hrib.Value,
            CreationMethod: CreationMethod.Migrator,
            Name: (LocalizedString)name);
        LogEvent(hrib, created);
        session.Events.StartStream<PlaylistInfo>(hrib, created);

        if (!string.IsNullOrEmpty(description))
        {
            var infoChanged = new PlaylistInfoChanged(
                PlaylistId: hrib.Value,
                Description: (LocalizedString?)description
            );
            session.Events.Append(hrib.Value, infoChanged);
            LogEvent(hrib, infoChanged);
        }

        videos ??= Enumerable.Empty<Hrib>();
        foreach(var video in videos)
        {
            var itemAdded = new PlaylistVideoAdded(hrib.Value, video.Value);
            LogEvent(hrib, itemAdded);
            session.Events.Append(hrib.Value, itemAdded);
        }

        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<PlaylistInfo>(hrib.Value))!;
    }

    public async Task<ProjectInfo> CreateProject(
        string name,
        Hrib projectGroupId,
        string? description = null,
        DateTime? releasedOn = default,
        bool isLocked = true,
        IEnumerable<ProjectAuthorInfo>? authors = default,
        Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();
        var created = new ProjectCreated(
            ProjectId: hrib.Value,
            CreationMethod: CreationMethod.Migrator,
            ProjectGroupId: projectGroupId.Value,
            Name: (LocalizedString)name);
        LogEvent(hrib, created);

        var infoChanged = new ProjectInfoChanged(
            ProjectId: hrib.Value,
            ReleasedOn: releasedOn.HasValue
                ? new DateTimeOffset(releasedOn.Value).ToUniversalTime()
                : null,
            Description: (LocalizedString?)description);
        LogEvent(hrib, infoChanged);

        session.Events.StartStream<ProjectInfo>(hrib, created, infoChanged);

        if (isLocked)
        {
            var locked = new ProjectLocked(hrib.Value);
            session.Events.Append(hrib.Value, locked);
            LogEvent(hrib, locked);
        }

        authors ??= Enumerable.Empty<ProjectAuthorInfo>();
        foreach(var author in authors)
        {
            var authorAdded = new ProjectAuthorAdded(
                ProjectId: hrib.Value,
                AuthorId: author.Id,
                Kind: author.Kind,
                Roles: author.Roles);
            session.Events.Append(hrib.Value, authorAdded);
            LogEvent(hrib, authorAdded);
        }

        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<ProjectInfo>(hrib.Value))!;
    }

    public async Task<ProjectGroupInfo> CreateProjectGroup(
        string name,
        Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();
        var created = new ProjectGroupCreated(
            ProjectGroupId: hrib.Value,
            CreationMethod.Migrator,
            (LocalizedString)name);
        LogEvent(hrib, created);

        session.Events.StartStream<ProjectGroupInfo>(hrib, created);

        var closed = new ProjectGroupClosed(hrib.Value);
        session.Events.Append(hrib.Value, closed);
        LogEvent(hrib, closed);

        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<ProjectGroupInfo>(hrib.Value))!;
    }

    private void LogEvent<TEvent>(Hrib hrib, TEvent @event)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("[{}]: {}", hrib, @event);
        }
    }
}
