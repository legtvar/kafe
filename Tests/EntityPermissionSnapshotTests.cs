using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events.Daemon;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Kafe.Data.Projections;
using Kafe.Media;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public class EntityPermissionSnapshotTests(ApiFixture fixture, ITestOutputHelper testOutput)
    : ApiTestBase(fixture, testOutput)
{
    private static readonly DateTimeOffset EventTimestamp = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task EntityPermissionEventProjection_AfterSeeding_MatchesSnapshots()
    {
        Assert.NotNull(ProjectionCoordinator);
        await ProjectionCoordinator.PauseAsync();

        try
        {
            await PopulateEvents();
            await RebuildPermissionProjection();
            await AssertEntityCounts();
            await Task.WhenAll(
                VerifyProjectionTable<EntityPermissionInfo>("EntityPermissionInfo"),
                VerifyProjectionTable<RoleMembersInfo>("RoleMembersInfo"));
        }
        finally
        {
            await ProjectionCoordinator.ResumeAsync();
        }
    }

    private async Task PopulateEvents()
    {
        await using var session = Store.LightweightSession();

        for (var organization = 1; organization <= 2; organization++)
        {
            var id = Id("snaporg", organization);
            session.Events.StartStream<OrganizationInfo>(
                id,
                new OrganizationCreated(
                    id,
                    CreationMethod.Manual,
                    LocalizedString.CreateInvariant($"Snapshot Organization {organization}"))
            );
            session.Events.Append(
                id,
                new OrganizationGlobalPermissionsChanged(id, organization == 1
                    ? Permission.Read
                    : Permission.Read | Permission.Inspect));
        }

        for (var group = 1; group <= 4; group++)
        {
            var id = Id("snapgrp", group);
            var organizationId = Id("snaporg", (group - 1) / 2 + 1);
            session.Events.StartStream<ProjectGroupInfo>(
                id,
                new ProjectGroupCreated(
                    id,
                    CreationMethod.Manual,
                    organizationId,
                    LocalizedString.CreateInvariant($"Snapshot Group {group}"))
            );
            session.Events.Append(
                id,
                new ProjectGroupGlobalPermissionsChanged(
                    id,
                    group % 2 == 0 ? Permission.Inspect : Permission.None));
        }

        for (var project = 1; project <= 16; project++)
        {
            var id = Id("snapprj", project);
            var groupId = Id("snapgrp", (project - 1) / 4 + 1);
            session.Events.StartStream<ProjectInfo>(
                id,
                new ProjectCreated(
                    id,
                    TestSeedData.UserHrib,
                    CreationMethod.Manual,
                    groupId,
                    LocalizedString.CreateInvariant($"Snapshot Project {project}"))
            );
        }

        for (var artifact = 1; artifact <= 16; artifact++)
        {
            var id = Id("snapart", artifact);
            session.Events.StartStream<ArtifactInfo>(
                id,
                new ArtifactCreated(
                    id,
                    CreationMethod.Manual,
                    LocalizedString.CreateInvariant($"Snapshot Artifact {artifact}"),
                    EventTimestamp)
            );

            for (var shard = 1; shard <= 2; shard++)
            {
                var shardNumber = (artifact - 1) * 2 + shard;
                var shardId = Id("snapshd", shardNumber);
                session.Events.StartStream<ImageShardInfo>(
                    shardId,
                    new ImageShardCreated(
                        shardId,
                        CreationMethod.Manual,
                        id,
                        new ImageInfo(".png", "image/png", "png", shardNumber, shardNumber + 1))
                );
            }
        }

        // Existing project streams can link to artifacts only after those artifact streams are saved.
        await session.SaveChangesAsync();
        for (var artifact = 1; artifact <= 16; artifact++)
        {
            var projectId = Id("snapprj", artifact);
            var artifactId = Id("snapart", artifact);
            session.Events.Append(projectId, new ProjectArtifactAdded(projectId, artifactId, null));
        }

        for (var role = 1; role <= 2; role++)
        {
            var id = Id("snaprol", role);
            var organizationId = Id("snaporg", role);
            session.Events.StartStream<RoleInfo>(
                id,
                new RoleCreated(
                    id,
                    CreationMethod.Manual,
                    organizationId,
                    LocalizedString.CreateInvariant($"Snapshot Role {role}")),
                new RolePermissionSet(
                    id,
                    organizationId,
                    role == 1 ? Permission.Inspect : Permission.Write),
                new RolePermissionSet(
                    id,
                    Id("snapprj", role),
                    Permission.Read | Permission.Review));
        }

        await session.SaveChangesAsync();

        // Existing account streams must be appended only after all entities and roles above exist.
        session.Events.Append(
            TestSeedData.UserHrib,
            new AccountPermissionSet(TestSeedData.UserHrib, Id("snaporg", 2), Permission.Inspect),
            new AccountRoleSet(TestSeedData.UserHrib, Id("snaprol", 1)),
            new AccountRoleSet(TestSeedData.UserHrib, Id("snaprol", 2)));
        session.Events.Append(
            TestSeedData.AdminHrib,
            new AccountRoleSet(TestSeedData.AdminHrib, Id("snaprol", 2)));

        // Rebuilding below uses these timestamps for every GrantedAt value, including seed data.
        session.QueueSqlCommand(
            $"UPDATE {Store.Options.Events.DatabaseSchemaName}.mt_events SET timestamp = ?",
            EventTimestamp);
        await session.SaveChangesAsync();
    }

    private async Task RebuildPermissionProjection()
    {
        var logger = Host.Server.Services.GetRequiredService<ILogger<EntityPermissionSnapshotTests>>();
        using var daemon = await Store.BuildProjectionDaemonAsync(logger: logger);
        await daemon.RebuildProjectionAsync<EntityPermissionEventProjection>(CancellationToken.None);
    }

    private async Task AssertEntityCounts()
    {
        await using var query = Store.QuerySession();
        Assert.Equal(2, await query.Query<OrganizationInfo>().CountAsync(x => x.Id.StartsWith("snaporg")));
        Assert.Equal(4, await query.Query<ProjectGroupInfo>().CountAsync(x => x.Id.StartsWith("snapgrp")));
        Assert.Equal(16, await query.Query<ProjectInfo>().CountAsync(x => x.Id.StartsWith("snapprj")));
        Assert.Equal(16, await query.Query<ArtifactInfo>().CountAsync(x => x.Id.StartsWith("snapart")));
        Assert.Equal(32, await query.Query<ImageShardInfo>().CountAsync(x => x.Id.StartsWith("snapshd")));
        Assert.Equal(40, await query.Query<EntityPermissionInfo>().CountAsync(x => x.Id.StartsWith("snap")));
        Assert.Equal(2, await query.Query<RoleMembersInfo>().CountAsync(x => x.Id.StartsWith("snaprol")));
    }

    private async Task VerifyProjectionTable<T>(string fileName)
        where T : class, IEntity
    {
        await using var query = Store.QuerySession();
        var documents = await query.Query<T>().OrderBy(x => x.Id).ToListAsync();
        await Verifier.Verify(documents)
            .UseDirectory("Snapshots")
            .UseFileName(fileName)
            .UseStrictJson()
            .DontScrubDateTimes()
            .OrderEnumerableBy<string>(x => x);
    }

    private static string Id(string prefix, int number) => $"{prefix}{number:0000}";
}
