using System;
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
    private readonly ProjectGroupService projectGroupService;
    private readonly RoleService roleService;
    private readonly ILogger<MigrationService> logger;

    public MigrationService(
        ILogger<MigrationService> logger,
        IDocumentSession db,
        AccountService accountService,
        AuthorService authorService,
        ProjectGroupService projectGroupService,
        RoleService roleService)
    {
        this.db = db;
        this.accountService = accountService;
        this.authorService = authorService;
        this.projectGroupService = projectGroupService;
        this.roleService = roleService;
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
        if (id == Hrib.Empty)
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
            return await db.Events.KafeAggregateRequiredStream<MigrationInfo>(@old.Id, token: token);
        }

        return Error.Unmodified(modified.Id, "A migration");
    }

    public async Task<Err<MigrationInfo>> CreateOrEdit(MigrationInfo info, CancellationToken token = default)
    {
        // TODO: Get rid of the unnecessary trip to DB (by calling Load twice).
        var existing = info.Id == Hrib.InvalidValue ? null : await Load(info.Id, token);
        return existing is null ? await Create(info, token) : await Edit(info, token);
    }

    public async Task<MigrationInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<MigrationInfo>(id.ToString(), token);
    }

    public async Task<ImmutableArray<MigrationInfo>> LoadMany(
        IEnumerable<Hrib> ids,
        CancellationToken token = default)
    {
        return (await db.KafeLoadManyAsync<MigrationInfo>(ids.ToImmutableArray(), token)).Unwrap();
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
        Hrib? AccountId,
        string OriginalId,
        string OriginalStorageName,
        ImmutableDictionary<string, string>? MigrationMetadata,
        string Email,
        string? Name,
        string? Uco,
        string? Phone,
        ImmutableArray<Hrib>? RoleIds
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

        if (entity is null && order.AccountId is not null)
        {
            entity = await accountService.Load(order.AccountId, token);
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
            var createResult = await accountService.Create(AccountInfo.Create(order.Email) with
            {
                Id = (order.AccountId ?? Hrib.Empty).RawValue,
                Name = order.Name,
                EmailAddress = order.Email,
                Uco = order.Uco,
                Phone = order.Phone,
                RoleIds = order.RoleIds is null
                    ? ImmutableArray<string>.Empty
                    : order.RoleIds.Value.Select(r => r.ToString()).ToImmutableArray()
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
                Phone = order.Phone ?? entity.Phone,
                RoleIds = order.RoleIds is null
                    ? ImmutableArray<string>.Empty
                    : order.RoleIds.Value.Select(r => r.ToString()).ToImmutableArray()
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
        Hrib? AuthorId,
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

        if (entity is null && order.AuthorId is not null)
        {
            entity = await authorService.Load(order.AuthorId, token);
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
            var createResult = await authorService.Create(AuthorInfo.Create(order.Name) with
            {
                Id = (order.AuthorId ?? Hrib.Empty).RawValue,
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

        var newOrModifiedMigration = (existingMigration ?? MigrationInfo.Create(order.OriginalStorageName, order.OriginalId, entity.Id)) with
        {
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
        Hrib? ProjectGroupId,
        string OriginalId,
        string OriginalStorageName,
        ImmutableDictionary<string, string>? MigrationMetadata,
        string Name,
        string? Description,
        Hrib OrganizationId,
        bool IsOpen,
        ImmutableDictionary<Hrib, Permission>? AccountPermissions,
        ImmutableDictionary<Hrib, Permission>? RolePermissions
    );

    public async Task<(MigrationInfo migration, ProjectGroupInfo projectGroup)> GetOrAddProjectGroup(
        ProjectGroupMigrationOrder order,
        CancellationToken token = default)
    {
        using var scope = logger.BeginScope("Migration of ProjectGroup '{}' ({}:{})",
            order.Name,
            order.OriginalStorageName,
            order.OriginalId);

        logger.LogInformation("Started");

        var existingMigration = await FindExistingMigration(
            typeof(ProjectGroupInfo),
            order.OriginalId,
            order.OriginalStorageName,
            token);

        ProjectGroupInfo? entity = null;
        if (entity is null && existingMigration is not null)
        {
            entity = await projectGroupService.Load(existingMigration.EntityId, token);
            logger.LogInformation(
                "Looking up by existing migration: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        if (entity is null && order.ProjectGroupId is not null)
        {
            entity = await projectGroupService.Load(order.ProjectGroupId, token);
            logger.LogInformation(
                "Looking up by ExistingId: {}",
                entity is null ? "Failed" : "Succeeded");
        }

        var orderName = LocalizedString.CreateInvariant(order.Name);
        if (entity is null)
        {
            var byName = await projectGroupService.List(new(Name: orderName), null, token);
            entity = byName.FirstOrDefault();
            logger.LogInformation(
                "Looking up by name: {}",
                entity is null ? "Failed" : "Succeeded");
            if (byName.Length > 1)
            {
                logger.LogWarning(
                    "Found multiple project groups with name '{}'. Manual adjustment recommended. Project groups: {}",
                    order.Name,
                    byName.Select(a => a.Id));
            }
        }

        entity ??= ProjectGroupInfo.Create(order.OrganizationId, orderName);
        entity = entity with
        {
            Id = order.ProjectGroupId?.ToString() ?? entity.Id,
            Name = orderName ?? entity.Name,
            Description = !string.IsNullOrEmpty(order.Description)
                ? LocalizedString.CreateInvariant(order.Description)
                : entity.Description,
            IsOpen = order.IsOpen
        };
        entity = (await projectGroupService.CreateOrEdit(entity, token)).Unwrap();

        var newOrModifiedMigration = (existingMigration ?? MigrationInfo.Create(order.OriginalStorageName, order.OriginalId, entity.Id)) with
        {
            MigrationMetadata = order.MigrationMetadata ?? ImmutableDictionary<string, string>.Empty
        };

        var migrationResult = existingMigration is null
            ? await Create(newOrModifiedMigration, token)
            : await Edit(newOrModifiedMigration, token);
        if (migrationResult.HasErrors)
        {
            throw migrationResult.AsException();
        }

        foreach (var accountPermission in order.AccountPermissions ?? ImmutableDictionary<Hrib, Permission>.Empty)
        {
            var res = await accountService.AddPermissions(
                accountPermission.Key,
                token,
                (entity.Id, accountPermission.Value));
            if (res.HasErrors)
            {
                logger.LogError(
                    res.AsException(),
                    "An error occurred while assigning permission to account '{}'.",
                    accountPermission.Key);
            }
        }

        foreach (var rolePermission in order.RolePermissions ?? ImmutableDictionary<Hrib, Permission>.Empty)
        {
            var res = await roleService.AddPermissions(
                rolePermission.Key,
                token,
                (entity.Id, rolePermission.Value));
            if (res.HasErrors)
            {
                logger.LogError(
                    res.AsException(),
                    "An error occurred while assigning permission to account '{}'.",
                    rolePermission.Key);
            }
        }

        return (migrationResult.Value, entity);
    }

    public record ProjectMigrationOrder(
        Hrib? ProjectId,
        string OriginalId,
        string OriginalStorageName,
        ImmutableDictionary<string, string>? MigrationMetadata,
        string Name,
        string? Description,
        Hrib ProjectGroupId,
        DateTimeOffset? ReleasedOn,
        bool IsLocked,
        ImmutableArray<ProjectAuthorInfo>? Authors,
        ImmutableDictionary<Hrib, Permission>? AccountPermissions,
        ImmutableDictionary<Hrib, Permission>? RolePermissions
    );

    public Task<ProjectInfo> GetOrAddProject(
        ProjectMigrationOrder order,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public record VideoMigrationOrder(
        Hrib? ArtifactId,
        Hrib? VideoShardId,
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
        Hrib? PlaylistId,
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
