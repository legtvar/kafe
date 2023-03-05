using Kafe.Data;
using Kafe.Data.Events;
using Kafe.Lemma;
using Kafe.Media;
using Marten;
using Npgsql;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using LemmaVideo = Kafe.Lemma.Video;

namespace Kafe.Migrator;

public static class Program
{
    private const string MigrationInfoFileName = "migration.json";

    private static IConfiguration configuration = null!;
    private static MigratorOptions migratorOptions = null!;
    private static WmaClient wma = null!;
    private static KafeClient kafe = null!;
    private static ILogger logger = null!;
    private static readonly ConcurrentDictionary<int, Hrib> authorMap = new();
    private static readonly ConcurrentDictionary<int, Hrib> projectMap = new();
    private static readonly ConcurrentDictionary<int, Hrib> artifactMap = new();
    private static readonly IMediaService mediaService = new FFmpegCoreService();


    public static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(c =>
            {
                c.AddJsonFile("appsettings.local.json");
            })
            .ConfigureServices((context, services) =>
            {
                WmaClient.AddWmaDb(services);
                Db.AddDb(services, context.Configuration, context.HostingEnvironment);
                services.AddSingleton<WmaClient>();
                services.AddSingleton<KafeClient>();
            })
            .Build();
        configuration = host.Services.GetRequiredService<IConfiguration>();
        migratorOptions = configuration.Get<MigratorOptions>()!;
        logger = host.Services.GetRequiredService<ILogger<Migrator>>();
        if (TryDropDb(host))
        {
            logger.LogInformation("Database dropped.");
        }

        wma = host.Services.GetRequiredService<WmaClient>();
        kafe = host.Services.GetRequiredService<KafeClient>();
        await kafe.Initialize();

        await MigrateAllAuthors();
        await MigrateAllProjectGroups();

        await DiscoverMigratedArtifacts();
        await MigrateAllVideos();

        await MigrateAllPlaylists();

        var authorCount = await kafe.CountAuthors();
        var playlistCount = await kafe.CountPlaylists();
        var projectGroupCount = await kafe.CountProjectGroups();
        var projectCount = await kafe.CountProjects();
        var artifactCount = await kafe.CountArtifacts();
        logger.LogInformation("Found {} authors, {} playlist, {} project groups, {} projects, {} artifacts.",
            authorCount,
            playlistCount,
            projectGroupCount,
            projectCount,
            artifactCount);

        await kafe.DisposeAsync();
        wma.Dispose();
    }

    private static async Task<Data.Aggregates.AuthorInfo> GetOrAddAuthor(
        int? id,
        string? name,
        string? uco,
        string? email,
        string? phone)
    {
        Hrib? hrib = id.HasValue ? authorMap.GetValueOrDefault(id.Value) : null;
        var kafeAuthor = await kafe.GetOrAddAuthor(hrib, name, uco, email, phone);
        if (id.HasValue)
        {
            authorMap.AddOrUpdate(id.Value, kafeAuthor.Id, (_, _) => kafeAuthor.Id);
        }
        return kafeAuthor;
    }

    private static async Task MigrateAllAuthors()
    {
        var authors = await wma.GetAllAuthors();
        foreach (var author in authors)
        {
            await GetOrAddAuthor(
                id: null,
                name: author.Name,
                uco: author.RoleTables.FirstOrDefault(r => r.Authoruco is not null)?.Authoruco?.ToString(),
                email: null,
                phone: null);

        }
    }

    private static async Task MigrateAllProjectGroups()
    {
        var projectGroups = await wma.GetAllProjectGroups();
        foreach (var projectGroup in projectGroups)
        {
            var kafeGroup = await kafe.CreateProjectGroup(projectGroup.Name);
            foreach (var project in projectGroup.Projects)
            {
                await MigrateProject(project, kafeGroup.Id);
            }
        }
    }

    private static async Task MigrateAllPlaylists()
    {
        var playlists = await wma.GetAllPlaylists();
        foreach (var playlist in playlists)
        {
            await MigratePlaylist(playlist);
        }
    }

    private static async Task<Data.Aggregates.ProjectInfo> MigrateProject(Project project, Hrib groupId)
    {
        var authors = ImmutableArray.CreateBuilder<Data.Aggregates.ProjectAuthor>();

        if (project.ExternalAuthorName is not null
            || project.ExternalAuthorMail is not null
            || project.ExternalAuthorUco is not null)
        {
            var author = await GetOrAddAuthor(
                null,
                project.ExternalAuthorName,
                project.ExternalAuthorUco,
                project.ExternalAuthorMail,
                project.ExternalAuthorPhone);
            authors.Add(new Data.Aggregates.ProjectAuthor(
                Id: author.Id,
                Kind: ProjectAuthorKind.Crew,
                Roles: ImmutableArray<string>.Empty));
        }


        foreach (var authorRole in project.RoleTables.GroupBy(r => r.Authoruco))
        {
            var first = authorRole.First();
            var kafeAuthor = await GetOrAddAuthor(
                first.Authoruco,
                first.Authorname,
                authorRole.Key.ToString(),
                null,
                null);
            var projectAuthor = new Data.Aggregates.ProjectAuthor(
                Id: kafeAuthor.Id,
                Kind: ProjectAuthorKind.Crew,
                Roles: authorRole.Select(r => r.Name).ToImmutableArray());
            authors.Add(projectAuthor);
        }

        var kafeProject = await kafe.CreateProject(
            name: project.Name,
            visibility: project.Publicpseudosecret == true ? Visibility.Internal : Visibility.Private,
            projectGroupId: groupId,
            description: project.Desc,
            releasedOn: project.ReleaseDate,
            isLocked: project.Closed == true,
            authors: authors);
        projectMap.AddOrUpdate(project.Id, kafeProject.Id, (_, _) => kafeProject.Id);
        return kafeProject;
    }

    private static async Task DiscoverMigratedArtifacts()
    {
        if (string.IsNullOrEmpty(migratorOptions.KafeVideosDirectory))
        {
            throw new InvalidOperationException("The 'KafeVideosDirectory' setting is not set.");
        }

        var videosDir = new DirectoryInfo(migratorOptions.KafeVideosDirectory);
        if (!videosDir.Exists)
        {
            videosDir.Create();
            return;
        }

        var subdirs = videosDir.GetDirectories();
        foreach (var subdir in subdirs)
        {
            var migrationInfoFile = subdir.GetFiles(MigrationInfoFileName).SingleOrDefault();

            if (migrationInfoFile is null)
            {
                logger.LogWarning($"'{subdir}' does not contain a {MigrationInfoFileName}.");
                continue;
            }

            try
            {
                using var stream = migrationInfoFile.OpenRead();
                var migrationInfo = await JsonSerializer.DeserializeAsync<VideoShardMigrationInfo>(stream);
                if (migrationInfo is null)
                {
                    logger.LogWarning($"'{migrationInfoFile}' could not be deserialized from JSON.");
                    continue;
                }

                var wmaVideo = await wma.GetVideo(migrationInfo.WmaId);
                if (wmaVideo is null)
                {
                    logger.LogWarning("Could not find a video with id '{}' in the WMA DB. " +
                        "This is a little weird since it was already migrated.", migrationInfo.WmaId);
                }
                else
                {
                    migrationInfo = migrationInfo with
                    {
                        Name = wmaVideo.Name,
                        AddedOn = wmaVideo.Adddate
                    };
                    await SerializedVideoShardMigrationInfo(migrationInfo);
                }

                artifactMap.AddOrUpdate(migrationInfo.WmaId, migrationInfo.ArtifactId, (_, _) => migrationInfo.ArtifactId);
                var originalVariant = await GetOriginalVariant(migrationInfo.VideoShardId);
                await kafe.CreateVideoArtifact(
                    name: migrationInfo.Name,
                    originalVariant: originalVariant,
                    artifactId: migrationInfo.ArtifactId,
                    shardId: migrationInfo.VideoShardId,
                    addedOn: migrationInfo.AddedOn ?? default);
            }
            catch (JsonException e)
            {
                logger.LogCritical(e, $"Could not read '{migrationInfoFile}'.");
                throw e;
            }
        }
    }

    private static async Task MigrateAllVideos()
    {
        var videos = await wma.GetAllVideos();
        foreach (var video in videos)
        {
            await MigrateVideo(video);
        }
    }

    private static async Task<Hrib> MigrateVideo(LemmaVideo video)
    {
        if (artifactMap.TryGetValue(video.Id, out var hrib))
        {
            if (projectMap.TryGetValue(video.Project, out var projectId))
            {
                await kafe.AddArtifactToProject(hrib, projectId);
            }
            return hrib;
        }

        var artifactId = Hrib.Create();
        var shardId = Hrib.Create();
        await CopyVideo(
            name: video.Name,
            addedOn: new DateTimeOffset(video.Adddate).ToUniversalTime(),
            wmaId: video.Id,
            artifactId: artifactId,
            shardId: shardId);

        var originalVariant = await GetOriginalVariant(shardId);

        await kafe.CreateVideoArtifact(
            name: video.Name,
            originalVariant: originalVariant,
            projectId: projectMap.GetValueOrDefault(video.Project),
            artifactId: artifactId,
            shardId: shardId,
            addedOn: new DateTimeOffset(video.Adddate).ToUniversalTime());


        artifactMap.AddOrUpdate(video.Id, artifactId, (_, _) => artifactId);

        return artifactId;
    }

    private static async Task CopyVideo(string name, DateTimeOffset addedOn, int wmaId, Hrib artifactId, Hrib shardId)
    {
        if (string.IsNullOrEmpty(migratorOptions.WmaVideosDirectory))
        {
            throw new InvalidOperationException("The 'WmaVideosDirectory' setting is not set.");
        }

        var originalDir = new DirectoryInfo(Path.Combine(migratorOptions.WmaVideosDirectory, wmaId.ToString()));
        if (!originalDir.Exists)
        {
            logger.LogError("Video '{}' ({}) could not be migrated because its directory could not be found on disk.",
                name, wmaId);
            return;
        }

        var originalCandidates = originalDir.GetFiles("original.*").OrderBy(f => f.Name).ToArray();
        if (originalCandidates.Length == 0)
        {
            logger.LogError("Video '{}' ({}) could not be migrated because the original footage could not be found.",
                name, wmaId);
            return;
        }

        if (originalCandidates.Length > 1)
        {
            logger.LogWarning("Video '{}' ({}) has multiple original footage files. Picking '{}'.",
                name, wmaId, originalCandidates.First().Name);
        }

        var originalFile = originalCandidates.First();

        var shardDir = new DirectoryInfo(Path.Combine(migratorOptions.KafeVideosDirectory!, shardId));
        if (shardDir.Exists)
        {
            logger.LogError("Video '{}' ({}) with shard id '{}' has already been migrated. " +
                "Hopefully this is not a hrib conflict.",
                name, wmaId, shardId);
            return;
        }

        shardDir.Create();

        var migrationInfo = new VideoShardMigrationInfo(
            WmaId: wmaId,
            ArtifactId: artifactId,
            VideoShardId: shardId,
            Name: name,
            AddedOn: addedOn);
        await SerializedVideoShardMigrationInfo(migrationInfo);

        var src = originalFile.OpenRead();
        using var dst = new FileStream(
            Path.Combine(shardDir.FullName, originalFile.Name),
            FileMode.Create,
            FileAccess.Write);
        await src.CopyToAsync(dst);
    }

    private static async Task SerializedVideoShardMigrationInfo(VideoShardMigrationInfo migrationInfo)
    {
        var shardDir = new DirectoryInfo(Path.Combine(
            migratorOptions.KafeVideosDirectory!,
            migrationInfo.VideoShardId));
        if (!shardDir.Exists)
        {
            throw new ArgumentException("Cannot serialize a migration info for video that is not migrated!");
        }

        using var metadataFile = new FileStream(
            Path.Combine(shardDir.FullName, MigrationInfoFileName),
            FileMode.Create,
            FileAccess.Write);
        await JsonSerializer.SerializeAsync(metadataFile, migrationInfo);
    }

    private static async Task<MediaInfo> GetOriginalVariant(Hrib shardId)
    {
        var shardDir = new DirectoryInfo(Path.Combine(migratorOptions.KafeVideosDirectory!, shardId));
        if (!shardDir.Exists)
        {
            logger.LogError($"Shard directory '{shardId}' does not exist. Video will be migrated without MediaInfo.");
            return MediaInfo.Invalid;
        }

        var originalCandidates = shardDir.GetFiles($"{Const.OriginalShardVariant}.*");
        if (originalCandidates.Length == 0 || originalCandidates.Length > 1)
        {
            throw new ArgumentException($"Shard '{shardId}' has either none or too many 'original' variants.");
        }

        var mediaInfo = await mediaService.GetInfo(originalCandidates.Single().FullName);
        return mediaInfo;
    }

    private static async Task<Data.Aggregates.PlaylistInfo> MigratePlaylist(Playlist playlist)
    {
        var artifactIds = ImmutableArray.CreateBuilder<Hrib>();
        foreach (var item in playlist.Items.OrderBy(i => i.Position))
        {
            if (!artifactMap.TryGetValue(item.Video, out var artifactId))
            {
                logger.LogWarning($"Video '{item.Video}' could not be found but is referenced by a playlist.");
                continue;
            }
            artifactIds.Add(artifactId);
        }

        return await kafe.CreatePlaylist(
            name: playlist.Name,
            description: playlist.Desc,
            videos: artifactIds.ToImmutable());
    }

    // knicked from https://github.com/JasperFx/marten/blob/master/src/CoreTests/create_database_Tests.cs
    private static bool TryDropDb(IHost host)
    {
        const string db = "kafe";
        try
        {
            using (var connection = new NpgsqlConnection(host.Services.GetRequiredService<IConfiguration>().GetConnectionString("postgres")))
            using (var cmd = connection.CreateCommand())
            {
                try
                {
                    connection.Open();
                    // Ensure connections to DB are killed - there seems to be a lingering idle session after AssertDatabaseMatchesConfiguration(), even after store disposal
                    cmd.CommandText +=
                        $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{db}' AND pid <> pg_backend_pid();";
                    cmd.CommandText += $"DROP DATABASE IF EXISTS {db};";
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        catch
        {
            return false;
        }
        return true;
    }


    private record Migrator;
}
