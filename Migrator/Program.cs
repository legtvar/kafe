using Kafe.Data;
using Kafe.Data.Events;
using Kafe.Lemma;
using Marten;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using LemmaVideo = Kafe.Lemma.Video;

namespace Kafe.Migrator;

public static class Program
{
    private static LemmaContext wma = null!;
    private static IDocumentStore martenStore = null!;
    private static IDocumentSession kafe = null!;
    private static ILogger logger = null!;
    private static Dictionary<int, Hrib> videoMap = new();

    public static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<LemmaContext>(options =>
                {
                    options.UseNpgsql(context.Configuration.GetConnectionString("WMA")
                        ?? throw new ArgumentException("The WMA connection string is missing!"));
                }
                );
                Db.AddDb(services, context.Configuration, context.HostingEnvironment);
            })
            .Build();
        logger = host.Services.GetRequiredService<ILogger<Migrator>>();
        if (TryDropDb(host))
        {
            logger.LogInformation("Database dropped.");
        }
        martenStore = host.Services.GetRequiredService<IDocumentStore>();
        await martenStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        kafe = martenStore.OpenSession();
        wma = host.Services.GetRequiredService<LemmaContext>();
        var projectGroups = wma.Projectgroups.OrderBy(a => a.Name)
            .Include(g => g.Projects)
            .ThenInclude(p => p.Videos)
            .ToList();
        foreach (var group in projectGroups)
        {
            MigrateProjectGroup(group);
        }
        await kafe.SaveChangesAsync();

        var playlists = wma.Playlists
            .Include(p => p.Items)
            .ToList();
        foreach (var playlist in playlists)
        {
            MigratePlaylist(playlist);
        }
        await kafe.SaveChangesAsync();

        var authorCount = await Marten.QueryableExtensions.CountAsync(
            kafe.Query<Kafe.Data.Aggregates.Author>());
        var playlistCount = await Marten.QueryableExtensions.CountAsync(
            kafe.Query<Kafe.Data.Aggregates.Playlist>());
        var projectGroupCount = await Marten.QueryableExtensions.CountAsync(
            kafe.Query<Kafe.Data.Aggregates.ProjectGroup>());
        var projectCounts = await Marten.QueryableExtensions.CountAsync(
            kafe.Query<Kafe.Data.Aggregates.Project>());
        logger.LogInformation($"Found {authorCount} authors, {playlistCount} playlist, {projectGroupCount} project groups, {projectCounts} projects.");

        await kafe.DisposeAsync();
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
        var stream = kafe.Events.StartStream<Kafe.Data.Aggregates.ProjectGroup>(hrib, created, closed);
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
        var stream = kafe.Events.StartStream<Kafe.Data.Aggregates.Project>(hrib, created, infoChanged);

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
        kafe.Events.StartStream<Kafe.Data.Aggregates.Playlist>(hrib, created);
        if (!string.IsNullOrEmpty(playlist.Desc)) {
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
        name ??= string.Format(Fallback.ProjectName, hrib);
        var created = new AuthorCreated(
            CreationMethod: CreationMethod.Migrator,
            Name: name);
        var infoChanged = new AuthorInfoChanged(
            Uco: uco,
            Email: email,
            Phone: phone);
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {infoChanged}");
        var stream = kafe.Events.StartStream<Kafe.Data.Aggregates.Author>(hrib, created, infoChanged);
        return hrib;
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
