using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
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
        session = martenStore.OpenSession();
    }

    public async ValueTask DisposeAsync()
    {
        martenStore.Dispose();
        await session.DisposeAsync();
    }

    public async Task<int> CountAuthors()
    {
        return await session.Query<Author>().CountAsync();
    }

    public async Task<int> CountProjectGroups()
    {
        return await session.Query<ProjectGroup>().CountAsync();
    }

    public async Task<int> CountProjects()
    {
        return await session.Query<Project>().CountAsync();
    }

    public async Task<int> CountPlaylists()
    {
        return await session.Query<Playlist>().CountAsync();
    }

    public async Task<int> CountArtifacts()
    {
        return await session.Query<Artifact>().CountAsync();
    }

    public async Task<Author> CreateAuthor(string? name, string? uco, string? email, string? phone, Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();
        name ??= string.Format(Fallback.AuthorName, hrib);
        var created = new AuthorCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: name);
        LogEvent(hrib, created);

        var infoChanged = new AuthorInfoChanged(
            Uco: uco,
            Email: email,
            Phone: phone);
        LogEvent(hrib, infoChanged);

        session.Events.StartStream<Author>(hrib, created, infoChanged);
        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<Author>(hrib))!;
    }

    public async Task<Author> GetOrAddAuthor(
        Hrib? hrib,
        string? name,
        string? uco,
        string? email,
        string? phone)
    {
        Author? author = null;
        if (hrib is not null )
        {
            author = await session.Events.AggregateStreamAsync<Author>(hrib);
        }
        else if (name is not null)
        {
            author = await session.Query<Author>().Where(a => a.Name == name).FirstOrDefaultAsync();
        }
        else if (uco is not null)
        {
            author = await session.Query<Author>().Where(a => a.Uco == uco).FirstOrDefaultAsync();
        }
        else if (email is not null)
        {
            author = await session.Query<Author>().Where(a => a.Email == email).FirstOrDefaultAsync();
        }
        else if (phone is not null)
        {
            author = await session.Query<Author>().Where(a => a.Phone == phone).FirstOrDefaultAsync();
        }

        if (author is null)
        {
            author = await CreateAuthor(name, uco, email, phone);
        }
        else
        {
            var infoChanged = new AuthorInfoChanged
            {
                Name = name,
                Uco = uco,
                Email = email,
                Phone = phone
            };
            if (name is not null || uco is not null || email is not null || phone is not null)
            {
                session.Events.Append(author.Id, infoChanged);
            }
        }
        await session.SaveChangesAsync();
        return author!;
    }

    public async Task<(Artifact, VideoShard)> CreateVideoArtifact(
        string name,
        Hrib? projectId = null,
        Hrib? artifactId = null,
        Hrib? shardId = null)
    {
        artifactId ??= Hrib.Create();
        var artifactCreated = new ArtifactCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: (LocalizedString)name);
        session.Events.StartStream<Artifact>(artifactId, artifactCreated);
        LogEvent(artifactId, artifactCreated);

        shardId ??= Hrib.Create();
        var shardCreated = new VideoShardCreated(
            CreationMethod: CreationMethod.Migrator,
            ArtifactId: artifactId);
        session.Events.StartStream<VideoShard>(shardId, shardCreated);
        LogEvent(shardId, shardCreated);

        if (projectId is not null)
        {
            await AddArtifactToProject(artifactId, projectId);
        }

        await session.SaveChangesAsync();
        return ((await session.Events.AggregateStreamAsync<Artifact>(artifactId))!,
            (await session.Events.AggregateStreamAsync<VideoShard>(shardId))!);
    }

    public async Task AddArtifactToProject(Hrib artifactId, Hrib projectId)
    {
        var artifactAdded = new ProjectArtifactAdded(artifactId);
        LogEvent(projectId, artifactAdded);
        session.Events.Append(projectId, artifactAdded);

        await session.SaveChangesAsync();
    }

    public async Task<Playlist> CreatePlaylist(
        string name,
        string? description,
        IEnumerable<Hrib>? videos,
        Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();

        var created = new PlaylistCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: (LocalizedString)name,
            Visibility: Visibility.Internal);
        LogEvent(hrib, created);
        session.Events.StartStream<Playlist>(hrib, created);

        if (!string.IsNullOrEmpty(description))
        {
            var infoChanged = new PlaylistInfoChanged(
                Description: (LocalizedString?)description
            );
            session.Events.Append(hrib, infoChanged);
            LogEvent(hrib, infoChanged);
        }

        videos ??= Enumerable.Empty<Hrib>();
        foreach(var video in videos)
        {
            var itemAdded = new PlaylistVideoAdded(video);
            LogEvent(hrib, itemAdded);
            session.Events.Append(hrib, itemAdded);
        }

        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<Playlist>(hrib))!;
    }

    public async Task<Project> CreateProject(
        string name,
        Visibility visibility,
        Hrib projectGroupId,
        string? description = null,
        DateTime? releaseDate = default,
        bool isLocked = true,
        IEnumerable<ProjectAuthor>? authors = default,
        Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();
        var created = new ProjectCreated(
            CreationMethod: CreationMethod.Migrator,
            ProjectGroupId: projectGroupId,
            Name: (LocalizedString)name,
            Visibility: visibility);
        LogEvent(hrib, created);

        var infoChanged = new ProjectInfoChanged(
            ReleaseDate: releaseDate.HasValue
                ? new DateTimeOffset(releaseDate.Value)
                : null,
            Description: (LocalizedString?)description);
        LogEvent(hrib, infoChanged);

        session.Events.StartStream<Project>(hrib, created, infoChanged);

        if (isLocked)
        {
            var locked = new ProjectLocked();
            session.Events.Append(hrib, locked);
            LogEvent(hrib, locked);
        }

        authors ??= Enumerable.Empty<ProjectAuthor>();
        foreach(var author in authors)
        {
            var authorAdded = new ProjectAuthorAdded(
                AuthorId: author.Id,
                Kind: author.Kind,
                Roles: author.Roles);
            session.Events.Append(hrib, authorAdded);
            LogEvent(hrib, authorAdded);
        }

        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<Project>(hrib))!;
    }

    public async Task<ProjectGroup> CreateProjectGroup(
        string name,
        Hrib? hrib = null)
    {
        hrib ??= Hrib.Create();
        var created = new ProjectGroupCreated(
            CreationMethod.Migrator,
            (LocalizedString)name);
        LogEvent(hrib, created);

        session.Events.StartStream<ProjectGroup>(hrib, created);

        var closed = new ProjectGroupClosed();
        session.Events.Append(hrib, closed);
        LogEvent(hrib, closed);

        await session.SaveChangesAsync();
        return (await session.Events.AggregateStreamAsync<ProjectGroup>(hrib))!;
    }

    private void LogEvent<TEvent>(Hrib hrib, TEvent @event)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("[{}]: {}", hrib, @event);
        }
    }
}
