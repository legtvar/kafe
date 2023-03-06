using Kafe.Api.Services;
using Kafe.Data;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Kafe.Data.Options;
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
        var accounts = scope.ServiceProvider.GetRequiredService<IAccountService>();
        foreach (var account in options.Value.Accounts)
        {
            var data = await accounts.LoadApiAccount(account.EmailAddress, token);
            if (data is null)
            {
                var id = await accounts.CreateTemporaryAccount(new(account.EmailAddress, account.PreferredCulture), token);
                data = await accounts.LoadApiAccount(id, token);
                if (data is null)
                {
                    logger.LogError("Seed account '{}' could not be created.", account.EmailAddress);
                    continue;
                }

                logger.LogInformation("Seed account '{}' created.", account.EmailAddress);
            }

            // TODO: THIS IS DUMB!!1! Configuration should just do its job properly
            var capabilities = account.Capabilities
                .Select(c => JsonSerializer.Deserialize<IAccountCapability>(c))
                .Where(c => c is not null)
                .Cast<IAccountCapability>()
                .ToImmutableHashSet();

            var missingCapabilities = capabilities.Except(data.Capabilities);

            if (missingCapabilities.Count > 0)
            {
                await accounts.AddCapabilities(data.Id, missingCapabilities, token);
                logger.LogInformation("Capabilities of seed account '{}' updated.", account.EmailAddress);
            }
        }
    }
}
