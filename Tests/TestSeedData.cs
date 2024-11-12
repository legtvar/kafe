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
    public const string Org1Hrib = "testorg0001";
    public const string Org2Hrib = "testorg0002";
    public const string Group1Hrib = "testgrp0001";
    public const string Group2Hrib = "testgrp0002";
    public const string Project1Hrib = "testprj0001";
    public const string Project2Hrib = "testprj0002";
    public const string Artifact1Hrib = "testart0001";

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
            OrganizationInfo.Create(LocalizedString.CreateInvariant("Test Organization 1")) with
            {
                Id = Org1Hrib
            },
            ct
        );
        await organizationService.Create(
            OrganizationInfo.Create(LocalizedString.CreateInvariant("Test Organization 2")) with
            {
                Id = Org2Hrib
            },
            ct
        );

        var projectGroupService = scope.ServiceProvider.GetRequiredService<ProjectGroupService>();
        await projectGroupService.Create(
            ProjectGroupInfo.Create(Org1Hrib, (LocalizedString)"Test Group 1") with
            {
                Id = Group1Hrib,
                IsOpen = true
            },
            ct
        );
        await projectGroupService.Create(
            ProjectGroupInfo.Create(Org2Hrib, (LocalizedString)"Test Group 2") with
            {
                Id = Group2Hrib,
                IsOpen = true
            },
            ct
        );

        var projectService = scope.ServiceProvider.GetRequiredService<ProjectService>();
        await projectService.Create(
            ProjectInfo.Create(Group1Hrib, (LocalizedString)"Test Project 1") with
            {
                Id = Project1Hrib
            },
            null,
            ct);
        await projectService.Create(
            ProjectInfo.Create(Group2Hrib, (LocalizedString)"Test Project 2") with
            {
                Id = Project2Hrib
            },
            null,
            ct);

        var artifactService = scope.ServiceProvider.GetRequiredService<ArtifactService>();
        await artifactService.Create(
            ArtifactInfo.Create(LocalizedString.CreateInvariant("Test Artifact 1")) with
            {
                Id = Artifact1Hrib
            },
            ct
        );

        logger.LogInformation("Test seed data populated.");
    }
}
