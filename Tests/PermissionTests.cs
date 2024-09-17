using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Kafe.Data.Projections;
using Marten.Events;
using Xunit;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public class PermissionTests(ApiFixture fixture) : ApiContext(fixture)
{
    [Fact]
    public async Task EntityPermissionInfo_System_ShouldExist()
    {
        using var daemon = await Store.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync<EntityPermissionEventProjection>(CancellationToken.None);
        await using var query = Store.QuerySession();
        var systemPerms = await query.KafeLoadAsync<EntityPermissionInfo>(Hrib.System);
        Assert.False(systemPerms.HasErrors);
        Assert.NotNull(systemPerms.Value);
    }

    [Fact]
    public async Task EntityPermissionInfo_RoleAssignment_ShouldSetAccountPerms()
    {
        var roleHrib = Hrib.Parse("testrole000").Unwrap().ToString();
        var expectedPerm = Permission.Read | Permission.Write | Permission.Administer;
        using (var session = Store.LightweightSession())
        {
            session.Events.StartStream<RoleInfo>(
                roleHrib,
                new RoleCreated(
                    roleHrib,
                    CreationMethod.Manual,
                    TestSeedData.TestOrganizationHrib,
                    LocalizedString.CreateInvariant("Test role")),
                new RolePermissionSet(
                    roleHrib,
                    TestSeedData.TestProjectHrib,
                    expectedPerm
                ));
            session.Events.Append(
                TestSeedData.UserHrib,
                new AccountRoleSet(TestSeedData.UserHrib, roleHrib));
            await session.SaveChangesAsync();
        }

        await Store.WaitForNonStaleProjectionDataAsync(TimeSpan.FromHours(1));

        using var query = Store.QuerySession();
        var projectPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.TestProjectHrib)).Unwrap();

        // NB: Check the explicit role permission exists
        Assert.NotEmpty(projectPerms.RoleEntries);
        Assert.True(projectPerms.RoleEntries.ContainsKey(roleHrib));
        Assert.True(projectPerms.RoleEntries[roleHrib].Sources.ContainsKey(TestSeedData.TestProjectHrib));
        Assert.Equal(expectedPerm, projectPerms.RoleEntries[roleHrib].Sources[TestSeedData.TestProjectHrib].Permission);

        // NB: Check the account permission implied by the role exists
        Assert.NotEmpty(projectPerms.AccountEntries);
        Assert.True(projectPerms.AccountEntries.ContainsKey(TestSeedData.UserHrib));
        Assert.True(projectPerms.AccountEntries[TestSeedData.UserHrib]
            .Sources.ContainsKey(roleHrib));
        Assert.Equal(expectedPerm, projectPerms.AccountEntries[TestSeedData.UserHrib]
            .Sources[roleHrib].Permission);
    }

    // TODO: Test case: Role has Inspect on a group and Review on an org.
    //       Role must have two sources on the group: the org and the group.
    //       The effective permission is Read | Inspect | Review. All members must also have this effective permission.
    //       All accounts in it must have Read | Inspect | Review on all of the group's projects
    //       with the role as source.
}
