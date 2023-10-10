using JasperFx.Core;
using Kafe.Api.Services;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Daemons;

public class SeedDaemon : BackgroundService
{
    private readonly IOptions<SeedOptions> options;
    private readonly IServiceProvider services;
    private readonly ILogger<SeedDaemon> logger;

    public SeedDaemon(
        IOptions<SeedOptions> options,
        IServiceProvider services,
        ILogger<SeedDaemon> logger)
    {
        this.options = options;
        this.services = services;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var scope = services.CreateScope();
        var accounts = scope.ServiceProvider.GetRequiredService<AccountService>();
        var projectGroups = scope.ServiceProvider.GetRequiredService<IProjectGroupService>();

        foreach (var account in options.Value.Accounts)
        {
            var data = await accounts.FindByEmail(account.EmailAddress, token);
            if (data is null)
            {
                var id = (await accounts.CreateTemporaryAccount(
                    account.EmailAddress,
                    account.PreferredCulture,
                    token)).Id;
                data = await accounts.FindByEmail(id, token);
                if (data is null)
                {
                    logger.LogError("Seed account '{}' could not be created.", account.EmailAddress);
                    continue;
                }

                logger.LogInformation("Seed account '{}' created.", account.EmailAddress);
            }

            var missingPermissions = account.Permissions
                .ToDictionary(p => p.Key, p => p.Value)
                .Except(data.Permissions)
                .Select(kv => (kv.Key, kv.Value))
                .ToImmutableArray();

            if (missingPermissions.Length > 0)
            {
                await accounts.AddPermissions(data.Id, missingPermissions, token);
                logger.LogInformation("Permissions of seed account '{}' updated.", account.EmailAddress);
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
            var creationInfo = new ProjectGroupCreationDto(name, null, deadline);
            // TODO: Remove the internal Create overload.
            var id = await ((ProjectGroupService)projectGroups).Create(creationInfo, group.Id, token);
            logger.LogInformation($"Seed project group '{id}' created.");
        }
    }
}
