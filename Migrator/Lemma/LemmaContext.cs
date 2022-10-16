using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kafe.Lemma;
public partial class LemmaContext : DbContext
{
    public LemmaContext()
    {
    }

    public LemmaContext(DbContextOptions<LemmaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appf> Appfs { get; set; } = null!;
    public virtual DbSet<Author> Authors { get; set; } = null!;
    public virtual DbSet<Ban> Bans { get; set; } = null!;
    public virtual DbSet<Collection> Collections { get; set; } = null!;
    public virtual DbSet<Emaillog> Emaillogs { get; set; } = null!;
    public virtual DbSet<Entry> Entries { get; set; } = null!;
    public virtual DbSet<Issuemaster> Issuemasters { get; set; } = null!;
    public virtual DbSet<OpeningHour> OpeningHours { get; set; } = null!;
    public virtual DbSet<PermissionCategory> PermissionCategories { get; set; } = null!;
    public virtual DbSet<Person> Persons { get; set; } = null!;
    public virtual DbSet<Playlist> Playlists { get; set; } = null!;
    public virtual DbSet<Playlistitem> Playlistitems { get; set; } = null!;
    public virtual DbSet<Project> Projects { get; set; } = null!;
    public virtual DbSet<ProjectPerson> ProjectPeople { get; set; } = null!;
    public virtual DbSet<ProjectReservationSource> ProjectReservationSources { get; set; } = null!;
    public virtual DbSet<Projectgroup> Projectgroups { get; set; } = null!;
    public virtual DbSet<Request> Requests { get; set; } = null!;
    public virtual DbSet<Reservation> Reservations { get; set; } = null!;
    public virtual DbSet<Reservationsystem> Reservationsystems { get; set; } = null!;
    public virtual DbSet<RoleTable> RoleTables { get; set; } = null!;
    public virtual DbSet<Source> Sources { get; set; } = null!;
    public virtual DbSet<SourceType> SourceTypes { get; set; } = null!;
    public virtual DbSet<SourcesReservation> SourcesReservations { get; set; } = null!;
    public virtual DbSet<Stateholiday> Stateholidays { get; set; } = null!;
    public virtual DbSet<Text> Texts { get; set; } = null!;
    public virtual DbSet<Vacation> Vacations { get; set; } = null!;
    public virtual DbSet<Variable> Variables { get; set; } = null!;
    public virtual DbSet<Video> Videos { get; set; } = null!;
    public virtual DbSet<Videolink> Videolinks { get; set; } = null!;
    public virtual DbSet<Vote> Votes { get; set; } = null!;
    public virtual DbSet<WmaProperty> WmaProperties { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appf>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => new { e.Person, e.Id })
                .HasName("bans_pkey");

            entity.HasOne(d => d.AdminNavigation)
                .WithMany(p => p.BanAdminNavigations)
                .HasForeignKey(d => d.Admin)
                .HasConstraintName("bans_admin_fkey");

            entity.HasOne(d => d.PersonNavigation)
                .WithMany(p => p.BanPersonNavigations)
                .HasForeignKey(d => d.Person)
                .HasConstraintName("bans_person_fkey");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");
        });

        modelBuilder.Entity<Emaillog>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Entry>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.VideoNavigation)
                .WithMany(p => p.Entries)
                .HasForeignKey(d => d.Video)
                .HasConstraintName("video_id_fk");
        });

        modelBuilder.Entity<Issuemaster>(entity =>
        {
            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");

            entity.Property(e => e.Room).HasDefaultValueSql("''::character varying");
        });

        modelBuilder.Entity<OpeningHour>(entity =>
        {
            entity.HasKey(e => new { e.Issuemaster, e.Id })
                .HasName("opening_hours_pkey");

            entity.HasOne(d => d.IssuemasterNavigation)
                .WithMany(p => p.OpeningHours)
                .HasForeignKey(d => d.Issuemaster)
                .HasConstraintName("opening_hours_issuemaster_fkey");
        });

        modelBuilder.Entity<PermissionCategory>(entity =>
        {
            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");

            entity.HasMany(d => d.People)
                .WithMany(p => p.PermissionCategories)
                .UsingEntity<Dictionary<string, object>>(
                    "PersonsPermissionCategory",
                    l => l.HasOne<Person>().WithMany().HasForeignKey("Person").HasConstraintName("persons__permission_categories_person_fkey"),
                    r => r.HasOne<PermissionCategory>().WithMany().HasForeignKey("PermissionCategory").HasConstraintName("persons__permission_categories_permission_category_fkey"),
                    j =>
                    {
                        j.HasKey("PermissionCategory", "Person").HasName("persons__permission_categories_pkey");

                        j.ToTable("persons__permission_categories", "lemma");

                        j.IndexerProperty<int>("PermissionCategory").HasColumnName("permission_category");

                        j.IndexerProperty<int>("Person").HasColumnName("person");
                    });
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Email).HasDefaultValueSql("''::character varying");

            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");

            entity.Property(e => e.Phone).HasDefaultValueSql("''::character varying");

            entity.Property(e => e.Status).HasDefaultValueSql("'REGISTRATION'::character varying");

            entity.HasMany(d => d.Issuemasters)
                .WithMany(p => p.People)
                .UsingEntity<Dictionary<string, object>>(
                    "IssuemastersPerson",
                    l => l.HasOne<Issuemaster>().WithMany().HasForeignKey("Issuemaster").HasConstraintName("issuemasters__persons_issuemaster_fkey"),
                    r => r.HasOne<Person>().WithMany().HasForeignKey("Person").HasConstraintName("issuemasters__persons_person_fkey"),
                    j =>
                    {
                        j.HasKey("Person", "Issuemaster").HasName("issuemasters__persons_pkey");

                        j.ToTable("issuemasters__persons", "lemma");

                        j.IndexerProperty<int>("Person").HasColumnName("person");

                        j.IndexerProperty<int>("Issuemaster").HasColumnName("issuemaster");
                    });
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Playlistitem>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.PlaylistNavigation)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.Playlist)
                .HasConstraintName("playlist_id_fk");

            entity.HasOne(d => d.VideoNavigation)
                .WithMany(p => p.Playlistitems)
                .HasForeignKey(d => d.Video)
                .HasConstraintName("video_id_fk");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.GroupNavigation)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.Group)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("projectgroup_id_fk");

            entity.HasOne(d => d.OwnerNavigation)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.Owner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("persons_id_fk");
        });

        modelBuilder.Entity<ProjectPerson>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.PersonNavigation)
                .WithMany(p => p.ProjectPeople)
                .HasForeignKey(d => d.Person)
                .HasConstraintName("person_id_fk");

            entity.HasOne(d => d.ProjectNavigation)
                .WithMany(p => p.ProjectPeople)
                .HasForeignKey(d => d.Project)
                .HasConstraintName("project_id_fk");
        });

        modelBuilder.Entity<ProjectReservationSource>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ProjectNavigation)
                .WithMany(p => p.ProjectReservationSources)
                .HasForeignKey(d => d.Project)
                .HasConstraintName("project_id_fk");

            entity.HasOne(d => d.ReservationNavigation)
                .WithMany(p => p.ProjectReservationSources)
                .HasForeignKey(d => d.Reservation)
                .HasConstraintName("reservation_id_fk");

            entity.HasOne(d => d.SourceNavigation)
                .WithMany(p => p.ProjectReservationSources)
                .HasForeignKey(d => d.Source)
                .HasConstraintName("source_id_fk");
        });

        modelBuilder.Entity<Projectgroup>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Daterequested).HasDefaultValueSql("statement_timestamp()");

            entity.HasOne(d => d.ModifiedbyNavigation)
                .WithMany(p => p.RequestModifiedbyNavigations)
                .HasForeignKey(d => d.Modifiedby)
                .HasConstraintName("modified_by_fkey");

            entity.HasOne(d => d.RequestedbyNavigation)
                .WithMany(p => p.RequestRequestedbyNavigations)
                .HasForeignKey(d => d.Requestedby)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("requested_by_fkey");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(e => e.Endnotified).HasDefaultValueSql("true");

            entity.Property(e => e.Notifybeforeend).HasDefaultValueSql("24");

            entity.Property(e => e.Notifybeforepickup).HasDefaultValueSql("24");

            entity.Property(e => e.Pickupnotified).HasDefaultValueSql("true");

            entity.Property(e => e.Status).HasDefaultValueSql("'RESERVED'::character varying");

            entity.HasOne(d => d.IssuemasterNavigation)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.Issuemaster)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reservations_issuemaster_fkey");

            entity.HasOne(d => d.PersonNavigation)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.Person)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reservations_person_fkey");
        });

        modelBuilder.Entity<RoleTable>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.AuthorucoNavigation)
                .WithMany(p => p.RoleTables)
                .HasForeignKey(d => d.Authoruco)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("author_id_fk");

            entity.HasOne(d => d.ProjectNavigation)
                .WithMany(p => p.RoleTables)
                .HasForeignKey(d => d.Project)
                .HasConstraintName("project_id_fk");
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.CollectionNavigation)
                .WithMany(p => p.Sources)
                .HasForeignKey(d => d.Collection)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("sources_collection_fkey");

            entity.HasOne(d => d.IssuemasterNavigation)
                .WithMany(p => p.Sources)
                .HasForeignKey(d => d.Issuemaster)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sources_issuemaster_fkey");

            entity.HasOne(d => d.TypeNavigation)
                .WithMany(p => p.Sources)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sources_type_fkey");
        });

        modelBuilder.Entity<SourceType>(entity =>
        {
            entity.Property(e => e.Name).HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.PermissionCategoryNavigation)
                .WithMany(p => p.SourceTypes)
                .HasForeignKey(d => d.PermissionCategory)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("source_types_permission_category_fkey");
        });

        modelBuilder.Entity<SourcesReservation>(entity =>
        {
            entity.HasKey(e => new { e.Source, e.Reservation })
                .HasName("sources__reservations_pkey");

            entity.HasOne(d => d.ReservationNavigation)
                .WithMany(p => p.SourcesReservations)
                .HasForeignKey(d => d.Reservation)
                .HasConstraintName("sources__reservations_reservation_fkey");

            entity.HasOne(d => d.SourceNavigation)
                .WithMany(p => p.SourcesReservations)
                .HasForeignKey(d => d.Source)
                .HasConstraintName("sources__reservations_source_fkey");
        });

        modelBuilder.Entity<Stateholiday>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Text>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Vacation>(entity =>
        {
            entity.HasKey(e => new { e.Issuemaster, e.Id })
                .HasName("vacations_pkey");

            entity.HasOne(d => d.IssuemasterNavigation)
                .WithMany(p => p.Vacations)
                .HasForeignKey(d => d.Issuemaster)
                .HasConstraintName("vacations_issuemaster_fkey");
        });

        modelBuilder.Entity<Variable>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Publicrequestdate).HasDefaultValueSql("'1990-01-01 12:00:00'::timestamp without time zone");

            entity.HasOne(d => d.ProjectNavigation)
                .WithMany(p => p.Videos)
                .HasForeignKey(d => d.Project)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_id_fk");

            entity.HasOne(d => d.ResponsiblepersonNavigation)
                .WithMany(p => p.Videos)
                .HasForeignKey(d => d.Responsibleperson)
                .HasConstraintName("persons_id_fk");
        });

        modelBuilder.Entity<Videolink>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.VideoNavigation)
                .WithMany(p => p.Videolinks)
                .HasForeignKey(d => d.Video)
                .HasConstraintName("video_id_fk");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.PersonNavigation)
                .WithMany(p => p.Votes)
                .HasForeignKey(d => d.Person)
                .HasConstraintName("persons_id_pk");

            entity.HasOne(d => d.VideoNavigation)
                .WithMany(p => p.Votes)
                .HasForeignKey(d => d.Video)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("video_id_fk");
        });

        modelBuilder.Entity<WmaProperty>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.HasSequence("appf_id_seq", "lemma");

        modelBuilder.HasSequence("emaillog_id_seq", "lemma");

        modelBuilder.HasSequence("entry_id_seq", "lemma");

        modelBuilder.HasSequence("playlist_id_seq", "lemma");

        modelBuilder.HasSequence("playlistitem_id_seq", "lemma");

        modelBuilder.HasSequence("project__person_id_seq", "lemma");

        modelBuilder.HasSequence("project__reservation__source_id_seq", "lemma");

        modelBuilder.HasSequence("project_id_seq", "lemma");

        modelBuilder.HasSequence("projectgroup_id_seq", "lemma");

        modelBuilder.HasSequence("request_id_seq", "lemma");

        modelBuilder.HasSequence("role_table_id_seq", "lemma");

        modelBuilder.HasSequence("stateholiday_id_seq", "lemma");

        modelBuilder.HasSequence("video_id_seq", "lemma");

        modelBuilder.HasSequence("videolink_id_seq", "lemma");

        modelBuilder.HasSequence("vote_id_seq", "lemma");

        modelBuilder.HasSequence("wma_property_id_seq", "lemma");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
