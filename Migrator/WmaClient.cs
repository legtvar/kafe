using Kafe.Lemma;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Migrator;

public sealed class WmaClient : IDisposable
{
    private readonly LemmaContext wma;

    public WmaClient(LemmaContext wma)
    {
        this.wma = wma;
    }

    public static IServiceCollection AddWmaDb(IServiceCollection services)
    {
        return services.AddDbContext<LemmaContext>((provider, options) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            options.UseNpgsql(configuration.GetConnectionString("WMA")
                ?? throw new ArgumentException("The WMA connection string is missing!"));
        });
    }

    public Task<List<Author>> GetAllAuthors()
    {
        return wma.Authors
            .Include(a => a.RoleTables)
            .ToListAsync();
    }

    public Task<List<Projectgroup>> GetAllProjectGroups()
    {
        return wma.Projectgroups.OrderBy(a => a.Name)
            .Include(g => g.Projects)
            .ThenInclude(p => p.RoleTables)
            .ThenInclude(r => r.AuthorucoNavigation)
            .ToListAsync();
    }

    public Task<List<Playlist>> GetAllPlaylists()
    {
        return wma.Playlists
            .Include(p => p.Items)
            .ToListAsync();
    }

    public Task<List<Video>> GetAllVideos()
    {
        return wma.Videos
            .Include(v => v.ProjectNavigation)
            .Include(v => v.ResponsiblepersonNavigation)
            .ToListAsync();
    }

    public void Dispose()
    {
        ((IDisposable)wma).Dispose();
    }
}
