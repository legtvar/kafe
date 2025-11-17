using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Aggregates;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafe.Data;

public class UserSeedData : IInitialData
{
    private readonly IOptions<StorageOptions> storageOptions;
    private readonly IOptions<SeedOptions> options;
    private readonly IServiceProvider services;
    private readonly ILogger<UserSeedData> logger;

    public UserSeedData(
        IOptions<StorageOptions> storageOptions,
        IOptions<SeedOptions> options,
        IServiceProvider services,
        ILogger<UserSeedData> logger
    )
    {
        this.storageOptions = storageOptions;
        this.options = options;
        this.services = services;
        this.logger = logger;
    }

    public async Task Populate(IDocumentStore store, CancellationToken token)
    {
        logger.LogInformation("Populating user seed data.");
        if (!storageOptions.Value.AllowSeedData)
        {
            logger.LogInformation("Skipping seed data since it is disabled in config.");
            return;
        }

        using var scope = services.CreateScope();
        var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
        var projectGroupService = scope.ServiceProvider.GetRequiredService<ProjectGroupService>();
        var organizationService = scope.ServiceProvider.GetRequiredService<OrganizationService>();

        foreach (var account in options.Value.Accounts)
        {
            var data = await accountService.FindByEmail(account.EmailAddress, token);
            if (data is null)
            {
                var res = await accountService.Create(
                    AccountInfo.Create(account.EmailAddress, account.PreferredCulture),
                    token: token
                );
                if (res.HasErrors)
                {
                    throw res.AsException();
                }

                logger.LogInformation("Seed account '{AccountEmailAddress}' created.", account.EmailAddress);
                data = res.Value;
            }

            if (account.Permissions is not null)
            {
                var missingPermissions = account.Permissions
                    .ToDictionary(p => p.Key, p => p.Value)
                    .Except(data.Permissions ?? ImmutableDictionary<string, Permission>.Empty)
                    .Select(kv => ((Hrib)kv.Key, kv.Value))
                    .ToImmutableArray();

                if (missingPermissions.Length > 0)
                {
                    await accountService.AddPermissions(data.Id, missingPermissions, token);
                    logger.LogInformation(
                        "Permissions of seed account '{AccountEmailAddress}' updated.",
                        account.EmailAddress
                    );
                }
            }
        }

        foreach (var organization in options.Value.Organizations)
        {
            if (string.IsNullOrWhiteSpace(organization.Name))
            {
                logger.LogWarning(
                    "Ignoring seed organization {@Organization} as its name is null or empty.",
                    organization
                );
                continue;
            }

            var name = LocalizedString.CreateInvariant(organization.Name);
            var existing = await organizationService.Load(organization.Id, token);
            if (existing is not null)
            {
                logger.LogDebug(
                    "Ignoring seed organization '{OrganizationId}' as it already exists.",
                    organization.Id
                );
                continue;
            }

            var createResult = await organizationService.Create(
                OrganizationInfo.Create(name) with
                {
                    Id = organization.Id,
                    CreationMethod = CreationMethod.Seed
                },
                token
            );
            if (createResult.HasErrors)
            {
                throw createResult.AsException();
            }

            logger.LogInformation("Seed organization '{OrganizationId}' created.", organization.Id);
        }

        foreach (var group in options.Value.ProjectGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                logger.LogWarning("Ignoring seed project group {@ProjectGroup} as its name is null or empty.", group);
                continue;
            }

            var name = LocalizedString.CreateInvariant(group.Name);
            var existing = await projectGroupService.Load(group.Id, token);
            if (existing is not null)
            {
                logger.LogDebug(
                    "Ignoring seed project group '{ProjectGroupId}' as it already exists.",
                    group.Id
                );
                continue;
            }

            var deadline = group.Deadline is null ? default : DateTimeOffset.Parse(group.Deadline);
            var createResult = await projectGroupService.Create(
                new ProjectGroupInfo(
                    Id: group.Id,
                    CreationMethod: CreationMethod.Seed,
                    OrganizationId: group.OrganizationId,
                    Name: LocalizedString.CreateInvariant(group.Name),
                    Description: null,
                    Deadline: deadline
                )
            );
            if (createResult.HasErrors)
            {
                throw createResult.AsException();
            }

            logger.LogInformation("Seed project group '{ProjectGroupId}' created.", group.Id);
            logger.LogInformation("User seed data populated.");
        }
    }
}
