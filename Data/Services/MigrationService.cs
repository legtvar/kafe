using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Common;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Media;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using Microsoft.Extensions.Logging;

namespace Kafe.Data.Services;

public class MigrationService
{
    private readonly IDocumentSession db;
    private readonly AccountService accountService;
    private readonly AuthorService authorService;
    private readonly ILogger<MigrationService> logger;

    public MigrationService(
        ILogger<MigrationService> logger,
        IDocumentSession db,
        AccountService accountService,
        AuthorService authorService)
    {
        this.db = db;
        this.accountService = accountService;
        this.authorService = authorService;
        this.logger = logger;
    }

    public async Task<Err<MigrationInfo>> Create(MigrationInfo @new, CancellationToken token = default)
    {
        var parseResult = Hrib.Parse(@new.Id);
        if (parseResult.HasErrors)
        {
            return parseResult.Errors;
        }

        var id = parseResult.Value;
        if (id == Hrib.Invalid)
        {
            id = Hrib.Create();
        }

        var created = new MigrationUndergone(
            MigrationId: id.ToString(),
            EntityId: @new.EntityId,
            OriginalStorageName: @new.OriginalStorageName,
            OriginalId: @new.OriginalId,
            MigrationMetadata: @new.MigrationMetadata);
        db.Events.KafeStartStream<MigrationInfo>(id, created);
        await db.SaveChangesAsync(token);

        return await db.Events.KafeAggregateRequiredStream<MigrationInfo>(id, token: token);
    }

    public async Task<Err<MigrationInfo>> Edit(MigrationInfo modified, CancellationToken token = default)
    {
        var @old = await Load(modified.Id, token);
        if (@old is null)
        {
            return Error.NotFound(modified.Id);
        }

        if (@old.EntityId != modified.EntityId
            || @old.OriginalStorageName != modified.OriginalStorageName
            || @old.OriginalId != modified.OriginalId
            || !@old.MigrationMetadata.SequenceEqual(modified.MigrationMetadata))
        {
            var amended = new MigrationAmended(
                MigrationId: @old.Id,
                OriginalStorageName: @old.OriginalStorageName,
                OriginalId: @old.OriginalId,
                MigrationMetadata: @old.MigrationMetadata);
            db.Events.Append(@old.Id, amended);
            await db.SaveChangesAsync(token);
            return await db.Events.AggregateStreamAsync<MigrationInfo>(@old.Id, token: token)
                ?? throw new InvalidOperationException($"The migration is no longer present in the database. "
                    + "This should never happend.");
        }

        return Error.Unmodified($"migration {modified.Id}");
    }

    public async Task<MigrationInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<MigrationInfo>(id.ToString(), token);
    }

    public async Task<ImmutableArray<MigrationInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.LoadManyAsync<MigrationInfo>(token, ids.Select(i => i.ToString())))
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

    public record AccountMigrationOrder(
        Hrib? ExistingId,
        string OriginalId,
        string OriginalStorageName,
        ImmutableDictionary<string, string>? MigrationMetadata,
        string Email,
        string? Name,
        string? Uco,
        string? Phone
    );

    public async Task<(MigrationInfo migration, AccountInfo account)> GetOrAddAccount(
        AccountMigrationOrder order,
        CancellationToken token = default)
    {
        using var scope = logger.BeginScope("Migration of account '{}' ({}:{})",
            order.Name,
            order.OriginalStorageName,
            order.OriginalId);

        logger.LogInformation("Started");

        var existingMigration = await FindExistingMigration(
            typeof(AccountInfo),
            order.OriginalId,
            order.OriginalStorageName,
            token);

        AccountInfo? entity = null;
        if (entity is null && existingMigration is not null)
        {
            entity = await accountService.Load(existingMigration.EntityId, token);
            logger.LogInformation(
                "Looking up by existing migration: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        if (entity is null && order.ExistingId is not null)
        {
            entity = await accountService.Load(order.ExistingId, token);
            logger.LogInformation(
                "Looking up by ExistingId: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        if (entity is null)
        {
            entity = await accountService.FindByEmail(order.Email, token);
            logger.LogInformation(
                "Looking up by Email: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        if (entity is null && !string.IsNullOrEmpty(order.Uco))
        {
            var byUco = await accountService.List(new(Uco: order.Uco), token);
            entity = byUco.FirstOrDefault();
            logger.LogInformation(
                "Looking up by Uco: {}",
                entity is null ? "Failed" : "Succeeded");
            if (byUco.Length > 1)
            {
                logger.LogWarning(
                    "Found multiple accounts with Uco '{}'. Manual adjustment recommended. Accounts: {}",
                    order.Uco,
                    byUco.Select(a => a.Id));
            }
        }

        if (entity is null)
        {
            var createResult = await accountService.Create(AccountInfo.Invalid with
            {
                Name = order.Name,
                EmailAddress = order.Email,
                Uco = order.Uco,
                Phone = order.Phone
            }, token);
            if (createResult.HasErrors)
            {
                throw createResult.AsException();
            }

            entity = createResult.Value;
        }
        else
        {
            entity = entity with
            {
                Name = order.Name ?? entity.Name,
                Uco = order.Uco ?? entity.Uco,
                EmailAddress = order.Email ?? entity.EmailAddress,
                Phone = order.Phone ?? entity.Phone
            };

            var editResult = await accountService.Edit(entity, token);
            if (editResult.HasErrors && editResult.Errors.Any(e => e.Id != Error.UnmodifiedId))
            {
                throw editResult.AsException();
            }

            entity = editResult.Value;
        }

        var newOrModifiedMigration = (existingMigration ?? MigrationInfo.Invalid) with
        {
            EntityId = entity.Id,
            OriginalId = order.OriginalId,
            OriginalStorageName = order.OriginalStorageName,
            MigrationMetadata = order.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty
        };

        var migrationResult = existingMigration is null
            ? await Create(newOrModifiedMigration, token)
            : await Edit(newOrModifiedMigration, token);
        if (migrationResult.HasErrors)
        {
            throw migrationResult.AsException();
        }

        return (migrationResult.Value, entity);
    }

    public record AuthorMigrationOrder(
        Hrib? ExistingId,
        string OriginalId,
        string OriginalStorageName,
        ImmutableDictionary<string, string>? MigrationMetadata,
        string? Email,
        string Name,
        string? Uco,
        string? Phone
    );

    public async Task<(MigrationInfo migration, AuthorInfo author)> GetOrAddAuthor(
        AuthorMigrationOrder order,
        CancellationToken token = default)
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

        AuthorInfo? entity = null;
        if (entity is null && existingMigration is not null)
        {
            entity = await authorService.Load(existingMigration.EntityId, token);
            logger.LogInformation(
                "Looking up by existing migration: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        if (entity is null && order.ExistingId is not null)
        {
            entity = await authorService.Load(order.ExistingId, token);
            logger.LogInformation(
                "Looking up by ExistingId: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        if (entity is null && !string.IsNullOrEmpty(order.Email))
        {
            var byEmail = await authorService.List(new(Email: order.Email), token);
            entity = byEmail.FirstOrDefault();
            logger.LogInformation(
                "Looking up by Email: {}",
                entity is null ? "Failed" : "Succeeded");
            if (byEmail.Length > 1)
            {
                logger.LogWarning(
                    "Found multiple authors with email '{}'. Manual adjustment recommended. Authors: {}",
                    order.Email,
                    byEmail.Select(a => a.Id));
            }
        }

        if (entity is null && !string.IsNullOrEmpty(order.Uco))
        {
            var byUco = await authorService.List(new(Uco: order.Uco), token);
            entity = byUco.FirstOrDefault();
            logger.LogInformation(
                "Looking up by Uco: {}",
                entity is null ? "Failed" : "Succeeded");
            if (byUco.Length > 1)
            {
                logger.LogWarning(
                    "Found multiple authors with Uco '{}'. Manual adjustment recommended. Authors: {}",
                    order.Uco,
                    byUco.Select(a => a.Id));
            }
        }

        if (entity is null)
        {
            var createResult = await authorService.Create(AuthorInfo.Invalid with
            {
                Name = order.Name,
                Email = order.Email,
                Uco = order.Uco,
                Phone = order.Phone
            }, token);
            if (createResult.HasErrors)
            {
                throw createResult.AsException();
            }

            entity = createResult.Value;
        }
        else
        {
            entity = entity with
            {
                Name = order.Name ?? entity.Name,
                Uco = order.Uco ?? entity.Uco,
                Email = order.Email ?? entity.Email,
                Phone = order.Phone ?? entity.Phone
            };

            var editResult = await authorService.Edit(entity, token);
            if (editResult.HasErrors && editResult.Errors.Any(e => e.Id != Error.UnmodifiedId))
            {
                throw editResult.AsException();
            }

            entity = editResult.Value;
        }

        var newOrModifiedMigration = (existingMigration ?? MigrationInfo.Invalid) with
        {
            EntityId = entity.Id,
            OriginalId = order.OriginalId,
            OriginalStorageName = order.OriginalStorageName,
            MigrationMetadata = order.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty
        };

        var migrationResult = existingMigration is null
            ? await Create(newOrModifiedMigration, token)
            : await Edit(newOrModifiedMigration, token);
        if (migrationResult.HasErrors)
        {
            throw migrationResult.AsException();
        }

        return (migrationResult.Value, entity);
    }

    public record ProjectGroupMigrationOrder(
        Hrib? ExistingId,
        string? OriginalId,
        string? OriginalStorageName,
        string Name,
        string? Description,
        Hrib OrganizationId,
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
        Hrib ProjectGroupId,
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
        Hrib OrganizationId,
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
