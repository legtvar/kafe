using Kafe.Api.Services;
using Kafe.Data;
using Kafe.Data.Events;
using Kafe.Data.Options;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var data = await accounts.Load(account.EmailAddress, token);
            if (data is not null)
            {
                logger.LogInformation("Seed account '{}' already exists.", account.EmailAddress);
                continue;
            }

            await accounts.CreateTemporaryAccount(new(account.EmailAddress, account.PreferredCulture), token);
            logger.LogInformation("Seed account '{}' created.", account.EmailAddress);
        }
    }
}
