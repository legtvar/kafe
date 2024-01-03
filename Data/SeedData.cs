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

public class SeedData : IInitialData
{
    private readonly IOptions<StorageOptions> storageOptions;
    private readonly IOptions<SeedOptions> options;
    private readonly IServiceProvider services;
    private readonly ILogger<SeedData> logger;

    public SeedData(
        IOptions<StorageOptions> storageOptions,
        IOptions<SeedOptions> options,
        IServiceProvider services,
        ILogger<SeedData> logger)
    {
        this.storageOptions = storageOptions;
        this.options = options;
        this.services = services;
        this.logger = logger;
    }

    public async Task Populate(IDocumentStore store, CancellationToken token)
    {
        if (!storageOptions.Value.AllowSeedData)
        {
            logger.LogInformation("Skipping seed data since it is disabled in config.");
            return;
        }

        using var scope = services.CreateScope();
        var accounts = scope.ServiceProvider.GetRequiredService<AccountService>();
        var projectGroups = scope.ServiceProvider.GetRequiredService<ProjectGroupService>();

        foreach (var account in options.Value.Accounts)
        {
            var data = await accounts.FindByEmail(account.EmailAddress, token);
            if (data is null)
            {
                var res = await accounts.CreateOrRefreshTemporaryAccount(
                    account.EmailAddress,
                    account.PreferredCulture,
                    token: token);
                data = await accounts.FindByEmail(res.Value.Id, token);
                if (data is null)
                {
                    logger.LogError("Seed account '{AccountEmailAddress}' could not be created.", account.EmailAddress);
                    continue;
                }

                logger.LogInformation("Seed account '{AccountEmailAddress}' created.", account.EmailAddress);
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
                    await accounts.AddPermissions(data.Id, missingPermissions, token);
                    logger.LogInformation(
                        "Permissions of seed account '{AccountEmailAddress}' updated.",
                        account.EmailAddress);
                }
            }
        }

        foreach (var group in options.Value.ProjectGroups)
        {
            if (group.Name is null)
            {
                continue;
            }

            var name = LocalizedString.CreateInvariant(group.Name);
            var existing = await projectGroups.Load(group.Id, token);
            if (existing is not null)
            {
                continue;
            }

            var deadline = group.Deadline is null ? default : DateTimeOffset.Parse(group.Deadline);
            var id = await projectGroups.Create(new ProjectGroupInfo(
                Id: group.Id,
                CreationMethod: CreationMethod.Seed,
                OrganizationId: group.OrganizationId,
                Name: LocalizedString.CreateInvariant(group.Name),
                Description: null,
                Deadline: deadline
            ));
            logger.LogInformation("Seed project group '{ProjectGroupId}' created.", id);
        }
    }
}
