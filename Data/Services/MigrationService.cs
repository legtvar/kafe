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
using Marten.Linq;
using Marten.Linq.MatchesSql;
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
        string? OriginalId = null,
        string? OriginalStorageName = null,
        Type? MigratedEntityType = null);

    public async Task<ImmutableArray<MigrationInfo>> List(
        MigrationFilter? filter = null,
        CancellationToken token = default)
    {
        var query = db.Query<MigrationInfo>();

        if (!string.IsNullOrEmpty(filter?.OriginalId))
        {
            query = (IMartenQueryable<MigrationInfo>)query
                .Where(m => m.OriginalId == filter.OriginalId);
        }

        if (!string.IsNullOrEmpty(filter?.OriginalStorageName))
        {
            query = (IMartenQueryable<MigrationInfo>)query
                .Where(m => m.OriginalStorageName == filter.OriginalStorageName);
        }

        if (filter?.MigratedEntityType is not null)
        {
            var tableName = await db.Database.ExistingTableFor(filter.MigratedEntityType);
            query = (IMartenQueryable<MigrationInfo>)query
                .Where(m => m.MatchesSql(
                    $"? = (SELECT type FROM mt_streams WHERE id = data ->> 'Id')",
                    tableName));
        }

        return (await query.ToListAsync(token)).ToImmutableArray();
    }

    public void LogSummary()
    {
        throw new NotImplementedException();
    }

    public record AuthorMigrationOrder(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string? Email,
        string? Name,
        string? Uco,
        string? Phone
    );

    public async Task<AuthorInfo> GetOrAddAuthor(AuthorMigrationOrder order, CancellationToken token = default)
    {
        using var scope = logger.BeginScope("Migration of author '{}' ({}:{})",
            order.Name,
            order.OriginalStorageName,
            order.OriginalId);

        logger.LogInformation("Started");

        var existingMigration = await FindExistingMigration(
            typeof(AuthorInfo),
            order.OriginalId,
            order.OriginalStorageName,
            token);

        AuthorInfo? existing = null;
        if (existing is null && existingMigration is not null)
        {
            existing = await authorService.Load(existingMigration.EntityId, token);
            logger.LogInformation(
                "Looking up by existing migration: {}",
                existing is null ? "Failed" : "Succeeded");
        }

        if (existing is null && order.ExistingId is not null)
        {
            existing = await authorService.Load(order.ExistingId, token);
            logger.LogInformation(
                "Looking up by ExistingId: {}",
                existing is null ? "Failed" : "Succeeded");
        }

        if (existing is null && !string.IsNullOrEmpty(order.Email))
        {
            var byEmail = await authorService.List(new(Email: order.Email), token);
            existing = byEmail.FirstOrDefault();
            logger.LogInformation(
                "Looking up by Email: {}",
                existing is null ? "Failed" : "Succeeded");
            if (byEmail.Length > 1)
            {
                logger.LogWarning(
                    "Found multiple authors with email '{}'. Manual adjustment recommended. Authors: {}",
                    order.Email,
                    byEmail.Select(a => a.Id));
            }
        }

        if (existing is null && !string.IsNullOrEmpty(order.Uco))
        {
            var byUco = await authorService.List(new(Uco: order.Uco), token);
            existing = byUco.FirstOrDefault();
            logger.LogInformation(
                "Looking up by Uco: {}",
                existing is null ? "Failed" : "Succeeded");
            if (byUco.Length > 1)
            {
                logger.LogWarning(
                    "Found multiple authors with Uco '{}'. Manual adjustment recommended. Authors: {}",
                    order.Uco,
                    byUco.Select(a => a.Id));
            }
        }

        if (existing is null)
        {
            return await authorService.Create(AuthorInfo.Invalid with
            {
                Name = order.Name,
                Email = order.Email,
                Uco = order.Uco,
                Phone = order.Phone
            }, token);
        }
    }

    public record ProjectGroupMigrationOrder(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        string? Description,
        bool IsLocked
    );

    public Task<ProjectGroupInfo> GetOrAddProjectGroup(
        ProjectGroupMigrationOrder order,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public record ProjectMigrationOrder(
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
        ProjectMigrationOrder order,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public record VideoMigrationOrder(
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
        VideoMigrationOrder order,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public record PlaylistMigrationOrder(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        string? Description,
        ImmutableArray<Hrib>? Videos
    );

    public Task<PlaylistInfo> GetOrAddPlaylist(PlaylistMigrationOrder order, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    private async Task<MigrationInfo?> FindExistingMigration(
        Type entityType,
        string? originalId,
        string? originalStorageName,
        CancellationToken token = default)
    {
        var existingMigrations = await List(
            new(
                OriginalId: originalId,
                OriginalStorageName: originalStorageName,
                MigratedEntityType: entityType
            ),
            token);

        if (existingMigrations.Length == 0)
        {
            logger.LogInformation("Existing migrations not found");
            return null;
        }

        if (existingMigrations.Length > 1)
        {
            var oldest = existingMigrations.MinBy(m => m.CreatedOn)!;
            logger.LogWarning(
                "Multiple existing migrations found. Using '{}', the oldest one. Manual adjustment recommended. Found migrations: {}",
                oldest.Id,
                existingMigrations.Select(m => m.Id));
            return oldest;
        }

        return existingMigrations.Single();
    }
}
