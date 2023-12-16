using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Media;
using Marten;
using Microsoft.Extensions.Logging;

namespace Kafe.Data.Services;

public class MigrationService
{
    private readonly IDocumentSession db;
    private readonly AuthorService authorService;
    private readonly ILogger<MigrationService> logger;

    public MigrationService(
        ILogger<MigrationService> logger,
        IDocumentSession db,
        AuthorService authorService)
    {
        this.db = db;
        this.authorService = authorService;
        this.logger = logger;
    }

    public async Task<MigrationInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<MigrationInfo>(id.Value, token);
    }

    public async Task<ImmutableArray<MigrationInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<MigrationInfo>(token, ids.Select(i => i.Value)))
            .Where(a => a is not null)
            .ToImmutableArray();
    }
    
    public record MigrationFilter(
        string? OriginalId,
        string? OriginalStorageName
    );

    public async Task<ImmutableArray<MigrationInfo>> List(
        MigrationFilter filter,
        CancellationToken token = default)
    {
        
    }

    public void LogSummary();

    record AuthorMigrationInfo(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string? Email,
        string? Name,
        string? Uco,
        string? Phone
    );

    public async Task<AuthorInfo> GetOrAddAuthor(AuthorMigrationInfo info, CancellationToken token = default)
    {
        var migration = await Load()
        
        AuthorInfo? existing = null;
        if (info.ExistingId is not null)
        {
            existing = await authorService.Load(info.ExistingId);
        }

        if (info.OriginalId is not null)
        {
            existing = await authorService.
        }
    }

    public record ProjectGroupMigrationInfo(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        string? Description,
        bool IsLocked
    );

    public Task<ProjectGroupInfo> GetOrAddProjectGroup(
        ProjectGroupMigrationInfo info,
        CancellationToken token = default);

    public record ProjectMigrationInfo(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        string? Description,
        Hrib? ProjectGroupId,
        DateTimeOffset? ReleasedOn,
        bool IsLocked,
        ImmutableArray<ProjectAuthorInfo>? Authors
    );

    public Task<ProjectInfo> GetOrAddProject(
        ProjectMigrationInfo info,
        CancellationToken token = default);

    public record VideoMigrationInfo(
        Hrib? ExistingArtifactId,
        Hrib? ExistingShardId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        MediaInfo OriginalVariant,
        DateTimeOffset AddedOn,
        Hrib? ContainingProjectId
    );

    public Task<(ArtifactInfo artifact, VideoShardInfo videoShard)> GetOrAddVideoArtifact(
        VideoMigrationInfo info,
        CancellationToken token = default);

    public record PlaylistMigrationInfo(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        string? Description,
        ImmutableArray<Hrib>? Videos
    );

    public Task<PlaylistInfo> GetOrAddPlaylist(PlaylistMigrationInfo info, CancellationToken token = default);
}
