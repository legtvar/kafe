using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Services;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Tests;

public class TestSeedData : IInitialData
{
    private readonly IServiceProvider services;

    public const string AdminHrib = "testadmin00";
    public const string AdminEmail = "admin@example.com";
    public const string UserHrib = "testuser000";
    public const string UserEmail = "user@example.com";
    public const string TestGroupHrib = "testgroup00";
    public const string TestProjectHrib = "testproject";

    public TestSeedData(IServiceProvider services)
    {
        this.services = services;
    }

    public async Task Populate(IDocumentStore store, CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();

        var admin = await accountService.CreateTemporaryAccount(AdminEmail, null, AdminHrib, ct);
        await accountService.AddPermissions(admin.Id, [(Hrib.System.Value, Permission.All)], ct);

        await accountService.CreateTemporaryAccount(UserEmail, null, UserHrib, ct);

        var projectGroupService = scope.ServiceProvider.GetRequiredService<ProjectGroupService>();
        await projectGroupService.Create((LocalizedString)"TestGroup", null, default, TestGroupHrib, ct);

        var projectService = scope.ServiceProvider.GetRequiredService<ProjectService>();
        await projectService.Create(TestGroupHrib, (LocalizedString)"TestProject", null, null, null, ct);
    }
}
