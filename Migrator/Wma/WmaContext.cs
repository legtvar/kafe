using Microsoft.EntityFrameworkCore;

namespace Kafe.Wma;

public class WmaContext : DbContext
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistItem> PlaylistItems => Set<PlaylistItem>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectGroup> ProjectGroups => Set<ProjectGroup>();
    public DbSet<Video> Videos => Set<Video>();
}
