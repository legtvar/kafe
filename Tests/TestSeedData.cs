using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafe.Tests;

public class TestSeedData : IInitialData
{
    private readonly IServiceProvider services;
    private readonly ILogger<TestSeedData> logger;
    public const string AdminHrib = "testadmin00";
    public const string AdminEmail = "admin@example.com";
    public const string UserHrib = "testuser000";
    public const string UserEmail = "user@example.com";
    public const string TestOrganizationHrib = "testorganiz";
    public const string TestGroupHrib = "testgroup00";
    public const string TestProjectHrib = "testproject";
    public const string TestArtifactHrib = "testartifac";

    public TestSeedData(IServiceProvider services, ILogger<TestSeedData> logger)
    {
        this.services = services;
        this.logger = logger;
    }

    public async Task Populate(IDocumentStore store, CancellationToken ct)
    {
        logger.LogInformation("Populating test seed data.");
        using var scope = services.CreateScope();
        var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();

        var admin = await accountService.CreateOrRefreshTemporaryAccount(AdminEmail, null, AdminHrib, ct);
        if (admin.HasErrors)
        {
            throw admin.AsException();
        }

        await accountService.AddPermissions(admin.Value.Id, [(Hrib.System.ToString(), Permission.All)], ct);

        await accountService.CreateOrRefreshTemporaryAccount(UserEmail, null, UserHrib, ct);

        var organizationService = scope.ServiceProvider.GetRequiredService<OrganizationService>();
        await organizationService.Create(
            OrganizationInfo.Create(LocalizedString.CreateInvariant("TestOrganization")) with
            {
                Id = TestOrganizationHrib
            },
            ct
        );

        var projectGroupService = scope.ServiceProvider.GetRequiredService<ProjectGroupService>();
        await projectGroupService.Create(
            ProjectGroupInfo.Create(TestOrganizationHrib, (LocalizedString)"TestGroup") with
            {
                Id = TestGroupHrib,
                IsOpen = true
            },
            ct
        );

        var projectService = scope.ServiceProvider.GetRequiredService<ProjectService>();
        await projectService.Create(
            ProjectInfo.Create(TestGroupHrib, (LocalizedString)"TestProject") with
            {
                Id = TestProjectHrib
            },
            null,
            ct);

        var artifactService = scope.ServiceProvider.GetRequiredService<ArtifactService>();
        await artifactService.Create(
            ArtifactInfo.Create(LocalizedString.CreateInvariant("TestArtifact")) with
            {
                Id = TestArtifactHrib
            },
            ct
        );

        logger.LogInformation("Test seed data populated.");
    }
}
