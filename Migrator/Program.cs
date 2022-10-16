using Kafe.Data;
using Kafe.Lemma;
using Marten;
using Marten.Events;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Weasel.Core;

namespace Kafe.Migrator;

public static class Program
{
    private static IHost host = null!;
    private static LemmaContext wma = null!;
    private static IDocumentStore martenStore = null!;
    private static IDocumentSession kafe = null!;
    private static ILogger logger = null!;

    public static async Task Main(string[] args)
    {
        host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<LemmaContext>(options =>
                {
                    options.UseNpgsql(context.Configuration.GetConnectionString("WMA"));
                }
                );
                services.AddMarten(options =>
                {
                    options.Connection(context.Configuration.GetConnectionString("KAFE"));
                    options.Events.StreamIdentity = StreamIdentity.AsString;
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                    options.CreateDatabasesForTenants(c =>
                    {
                        c.MaintenanceDatabase(context.Configuration.GetConnectionString("postgres"));
                        c.ForTenant()
                            .CheckAgainstPgDatabase()
                            .WithOwner("postgres")
                            .WithEncoding("UTF-8")
                            .ConnectionLimit(-1);
                    });
                });
            })
            .Build();
        logger = host.Services.GetRequiredService<ILogger<Migrator>>();
        if (TryDropDb())
        {
            logger.LogInformation("Database dropped.");
        }
        martenStore = host.Services.GetRequiredService<IDocumentStore>();
        await martenStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        kafe = martenStore.OpenSession();
        wma = host.Services.GetRequiredService<LemmaContext>();
        var projectGroups = wma.Projectgroups.OrderBy(a => a.Name).Include(g => g.Projects).ToList();
        foreach (var group in projectGroups)
        {
            MigrateProjectGroup(group);
        }
        await kafe.SaveChangesAsync();
        await kafe.DisposeAsync();
    }

    private static Hrib MigrateProjectGroup(Projectgroup group)
    {
        var hrib = Hrib.Create();
        var created = new ProjectGroupCreated(CreationMethod.Migrator);
        var infoChanged = new ProjectGroupInfoChanged(group.Name);
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {infoChanged}");
        var stream = kafe.Events.StartStream(hrib, created, infoChanged);
        foreach (var project in group.Projects)
        {
            MigrateProject(project, hrib);
        }
        return hrib;
    }

    private static Hrib MigrateProject(Project project, Hrib groupId)
    {
        var hrib = Hrib.Create();
        var created = new ProjectCreated(CreationMethod.Migrator, groupId);
        var infoChanged = new ProjectInfoChanged(
            Name: project.Name,
            Description: project.Desc,
            Visibility: project.Publicpseudosecret == true ? ProjectVisibility.Internal : ProjectVisibility.Private,
            ReleaseDate: project.ReleaseDate,
            Link: project.Web);
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {infoChanged}");
        var stream = kafe.Events.StartStream(hrib, created, infoChanged);

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
            var authorAdded = new ProjectAuthorAdded(authorId);
            logger.LogInformation($"[{hrib}]: {authorAdded}");
            kafe.Events.Append(hrib, authorAdded);
        }
        return hrib;
    }

    private static Hrib CreateAuthor(string? name, string? uco, string? email, string? phone)
    {
        var hrib = Hrib.Create();
        var created = new AuthorCreated(CreationMethod.Migrator);
        var infoChanged = new AuthorInfoChanged(
            Name: name,
            Uco: uco,
            Email: email,
            Phone: phone);
        logger.LogInformation($"[{hrib}]: {created}");
        logger.LogInformation($"[{hrib}]: {infoChanged}");
        var stream = kafe.Events.StartStream(hrib, created, infoChanged);
        return hrib;
    }

    // knicked from https://github.com/JasperFx/marten/blob/master/src/CoreTests/create_database_Tests.cs
    private static bool TryDropDb()
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
