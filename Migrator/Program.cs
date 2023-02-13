using Kafe.Data;
using Kafe.Data.Events;
using Kafe.Lemma;
using Marten;
using Npgsql;
using System.Text.Json;
using LemmaVideo = Kafe.Lemma.Video;

namespace Kafe.Migrator;

public static class Program
{
    private const string MigrationInfoFileName = "migration.json";

    private static IConfiguration configuration = null!;
    private static WmaClient wma = null!;
    private static IDocumentStore martenStore = null!;
    private static IDocumentSession kafe = null!;
    private static ILogger logger = null!;
    private static Dictionary<int, Hrib> authorMap = new();
    private static Dictionary<int, Hrib> videoMap = new();
    private static Dictionary<int, Hrib> artifactMap = new();


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
            })
            .Build();
        configuration = host.Services.GetRequiredService<IConfiguration>();
        logger = host.Services.GetRequiredService<ILogger<Migrator>>();
        if (TryDropDb(host))
        {
            logger.LogInformation("Database dropped.");
        }
        martenStore = host.Services.GetRequiredService<IDocumentStore>();
        await martenStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        kafe = martenStore.OpenSession();
        wma = host.Services.GetRequiredService<WmaClient>();

        await DiscoverMigratedArtifacts();
        await MigrateAllAuthors();
        await MigrateAllProjectGroups();
        await MigrateAllPlaylists();

        await kafe.SaveChangesAsync();

        //var projectGroups = await wma.GetAllProjectGroups();
        //foreach (var group in projectGroups)
        //{
        //    MigrateProjectGroup(group);
        //}
        //await kafe.SaveChangesAsync();

        //var playlists = await wma.GetAllPlaylists();
        //foreach (var playlist in playlists)
        //{
        //    MigratePlaylist(playlist);
        //}

        var authorCount = await QueryableExtensions.CountAsync(
            kafe.Query<Data.Aggregates.Author>());
        var playlistCount = await QueryableExtensions.CountAsync(
            kafe.Query<Data.Aggregates.Playlist>());
        var projectGroupCount = await QueryableExtensions.CountAsync(
            kafe.Query<Data.Aggregates.ProjectGroup>());
        var projectCounts = await QueryableExtensions.CountAsync(
            kafe.Query<Data.Aggregates.Project>());
        logger.LogInformation($"Found {authorCount} authors, {playlistCount} playlist, {projectGroupCount} project groups, {projectCounts} projects.");

        await kafe.DisposeAsync();
    }

    private static async Task MigrateAllAuthors()
    {
        var authors = await wma.GetAllAuthors();
        foreach (var author in authors)
        {
            var id = CreateAuthor(
                name: author.Name,
                uco: author.RoleTables.FirstOrDefault(r => r.Authoruco is not null)?.Authoruco?.ToString(),
                email: null,
                phone: null);
            authorMap.Add(author.Id, id);
        }
        await kafe.SaveChangesAsync();
    }

    private static async Task<Data.Aggregates.Author?> GetOrAddAuthor(
        int? id,
        string? name,
        string? uco,
        string? email,
        string? phone)
    {
        Data.Aggregates.Author? author = null;
        if (id is not null && authorMap.TryGetValue(id.Value, out var hrib))
        {
            author = await kafe.Events.AggregateStreamAsync<Data.Aggregates.Author>(hrib)!;
        }
        else if (name is not null)
        {
            author = await kafe.Query<Data.Aggregates.Author>().Where(a => a.Name == name).FirstOrDefaultAsync();
        }
        else if (uco is not null)
        {
            author = await kafe.Query<Data.Aggregates.Author>().Where(a => a.Uco == uco).FirstOrDefaultAsync();
        }
        else if (email is not null)
        {
            author = await kafe.Query<Data.Aggregates.Author>().Where(a => a.Email == email).FirstOrDefaultAsync();
        }
        else if (phone is not null)
        {
            author = await kafe.Query<Data.Aggregates.Author>().Where(a => a.Phone == phone).FirstOrDefaultAsync();
        }

        if (author is null)
        {
            var newHrib = CreateAuthor(name, uco, email, phone);
            author = await kafe.Events.AggregateStreamAsync<Data.Aggregates.Author>(newHrib);
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
                kafe.Events.Append(author.Id, infoChanged);
            }
        }
        await kafe.SaveChangesAsync();
        return author;
    }

    private static async Task MigrateAllProjectGroups()
    {
        var projectGroups = await wma.GetAllProjectGroups();
    }

    private static async Task MigrateAllPlaylists()
    {
        var playlists = await wma.GetAllPlaylists();
    }

    private static Hrib MigrateProjectGroup(Projectgroup group)
    {
        var hrib = Hrib.Create();
        var created = new ProjectGroupCreated(
            CreationMethod.Migrator,
            (LocalizedString)(group.Name ?? string.Format(Fallback.ProjectGroupName, hrib)));
        var closed = new ProjectGroupClosed();
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {closed}");
        var stream = kafe.Events.StartStream<Data.Aggregates.ProjectGroup>(hrib, created, closed);
        foreach (var project in group.Projects)
        {
            MigrateProject(project, hrib);
        }
        return hrib;
    }

    private static Hrib MigrateProject(Project project, Hrib groupId)
    {
        var hrib = Hrib.Create();
        var created = new ProjectCreated(
            CreationMethod: CreationMethod.Migrator,
            ProjectGroupId: groupId,
            Name: (LocalizedString)(project.Name ?? string.Format(Fallback.ProjectName, hrib)),
            Visibility: project.Publicpseudosecret == true ? Visibility.Internal : Visibility.Private);
        var infoChanged = new ProjectInfoChanged(
            ReleaseDate: project.ReleaseDate.HasValue
                ? new DateTimeOffset(project.ReleaseDate.Value)
                : null,
            Description: (LocalizedString?)project.Desc);
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {infoChanged}");
        var stream = kafe.Events.StartStream<Data.Aggregates.Project>(hrib, created, infoChanged);

        if (project.Closed == true)
        {
            var locked = new ProjectLocked();
            kafe.Events.Append(hrib, locked);
            logger.LogInformation($"[{hrib}]: {locked}");
        }

        if (project.ExternalAuthorName is not null
            || project.ExternalAuthorMail is not null
            || project.ExternalAuthorUco is not null)
        {
            var authorId = CreateAuthor(
                project.ExternalAuthorName,
                project.ExternalAuthorUco,
                project.ExternalAuthorMail,
                project.ExternalAuthorPhone);
            var authorAdded = new ProjectAuthorAdded(authorId, ProjectAuthorKind.Unknown);
            logger.LogInformation($"[{hrib}]: {authorAdded}");
            kafe.Events.Append(hrib, authorAdded);
        }

        foreach (var video in project.Videos)
        {
            MigrateVideo(video, hrib);
        }
        return hrib;
    }

    private static async Task DiscoverMigratedArtifacts()
    {
        var artifactDirectory = configuration.Get<MigratorConfiguration>()?.ArtifactDirectory;
        if (string.IsNullOrEmpty(artifactDirectory))
        {
            throw new InvalidOperationException("The 'ArtifactDirectory' setting is not set.");
        }

        var subdirs = new DirectoryInfo(artifactDirectory).GetDirectories();
        foreach (var subdir in subdirs)
        {
            var migrationInfoFile = subdir.GetFiles(MigrationInfoFileName).SingleOrDefault();

            if (migrationInfoFile is null)
            {
                logger.LogWarning($"'{subdir}' does not contain a {MigrationInfoFileName}.");
                continue;
            }

            using var stream = migrationInfoFile.OpenRead();
            var migrationInfo = await JsonSerializer.DeserializeAsync<ArtifactMigrationInfo>(stream);
            if (migrationInfo is null)
            {
                logger.LogWarning($"'{migrationInfoFile}' could not be deserialized from JSON.");
                continue;
            }

            artifactMap.Add(migrationInfo.WmaId, migrationInfo.KafeId);
        }
    }

    private static Hrib MigrateVideo(LemmaVideo video, Hrib projectId)
    {
        var hrib = Hrib.Create();
        var added = new ProjectArtifactAdded(
            ArtifactId: hrib);
        videoMap.Add(video.Id, hrib);
        logger.LogInformation($"[{projectId}]: {added}");
        kafe.Events.Append(projectId, added);
        return hrib;
    }

    private static Hrib MigratePlaylist(Playlist playlist)
    {
        var hrib = Hrib.Create();
        var created = new PlaylistCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: (LocalizedString)(playlist.Name ?? string.Format(Fallback.PlaylistName, hrib)),
            Visibility: Visibility.Internal);
        logger.LogInformation($"[{hrib}]: {created}");
        kafe.Events.StartStream<Data.Aggregates.Playlist>(hrib, created);
        if (!string.IsNullOrEmpty(playlist.Desc))
        {
            var infoChanged = new PlaylistInfoChanged(
                Description: (LocalizedString?)playlist.Desc
            );
            kafe.Events.Append(hrib, infoChanged);
            logger.LogInformation($"[{hrib}]: {infoChanged}");
        }

        foreach (var item in playlist.Items.OrderBy(i => i.Position))
        {
            if (!videoMap.TryGetValue(item.Video, out var videoId))
            {
                logger.LogWarning($"Video '{item.Video}' has not been migrated but is referenced by a playlist.");
                continue;
            }
            var itemAdded = new PlaylistVideoAdded(videoId);
            logger.LogInformation($"[{hrib}]: {itemAdded}");
            kafe.Events.Append(hrib, itemAdded);
        }
        return hrib;
    }

    private static Hrib CreateAuthor(string? name, string? uco, string? email, string? phone)
    {
        var hrib = Hrib.Create();
        name ??= string.Format(Fallback.AuthorName, hrib);
        var created = new AuthorCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: name);
        var infoChanged = new AuthorInfoChanged(
            Uco: uco,
            Email: email,
            Phone: phone);
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {infoChanged}");
        kafe.Events.StartStream<Data.Aggregates.Author>(hrib, created, infoChanged);
        return hrib;
    }

    private static Hrib CreateArtifact(string name, Hrib projectId, Hrib? id = null)
    {
        id ??= Hrib.Create();
        var created = new ArtifactCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: (LocalizedString)name,
            ProjectId: projectId);
        logger.LogInformation($"[{id}]: {created}");
        return id;
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
