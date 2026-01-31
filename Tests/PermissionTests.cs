using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.CodeGeneration.Model;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Documents;
using Kafe.Data.Events;
using Kafe.Data.Projections;
using Marten;
using Xunit;
using Xunit.Abstractions;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public class PermissionTests(ApiFixture fixture, ITestOutputHelper testOutput) : ApiTestBase(fixture, testOutput)
{
    [Fact]
    public async Task EntityPermissionInfo_System_ShouldExist()
    {
        await WaitForProjections();
        await using var query = Store.QuerySession();

        var systemPerms = await query.KafeLoadAsync<EntityPermissionInfo>(Hrib.System);
        Assert.False(systemPerms.HasError);
        Assert.NotNull(systemPerms.Value);
    }

    [Fact]
    public async Task EntityPermissionInfo_System_ShouldHaveAdmin()
    {
        await WaitForProjections();
        await using var query = Store.QuerySession();
        var systemPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(Hrib.System)).Unwrap();
        AssertAccountPermission(
            perms: systemPerms,
            accountHrib: TestSeedData.AdminHrib,
            permission: Permission.All,
            sourceHrib: Hrib.System);
    }

    [Fact]
    public async Task EntityPermissionInfo_All_ShouldHaveAdmin()
    {
        await WaitForProjections();
        await using var query = Store.QuerySession();
        var allPerms = query.Query<EntityPermissionInfo>().ToAsyncEnumerable();
        await foreach (var perms in allPerms)
        {
            var expected = perms.Id == Hrib.System
                ? Permission.All
                : Permission.Read | Permission.Append | Permission.Inheritable;
            AssertAccountPermission(
                perms: perms,
                accountHrib: TestSeedData.AdminHrib,
                permission: expected,
                sourceHrib: Hrib.System
            );
        }
    }

    [Fact]
    public async Task EntityPermissionInfo_RoleAssignment_ShouldSetAccountPerms()
    {
        var roleHrib = Hrib.Parse("testrole000").ToString();
        var expectedPerm = Permission.Read | Permission.Write | Permission.Administer;
        await using (var session = Store.LightweightSession())
        {
            session.Events.StartStream<RoleInfo>(
                roleHrib,
                new RoleCreated(
                    roleHrib,
                    CreationMethod.Manual,
                    TestSeedData.Org1Hrib,
                    LocalizedString.CreateInvariant("Test role")),
                new RolePermissionSet(
                    roleHrib,
                    TestSeedData.Project1Hrib,
                    expectedPerm
                ));
            // NB: For whatever reason Marten 7.28 would order the following appended event *before* the StartStream
            // ones above, thus this Save needs to happen here as well.
            await session.SaveChangesAsync();

            session.Events.Append(
                TestSeedData.UserHrib,
                new AccountRoleSet(TestSeedData.UserHrib, roleHrib));
            await session.SaveChangesAsync();
        }

        await WaitForProjections();

        await using var query = Store.QuerySession();

        var membersInfo = (await query.KafeLoadAsync<RoleMembersInfo>(roleHrib)).Unwrap();
        Assert.NotEmpty(membersInfo.MemberIds);
        Assert.Contains(TestSeedData.UserHrib, membersInfo.MemberIds);

        var projectPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Project1Hrib)).Unwrap();

        // NB: Check the explicit role permission exists
        Assert.NotEmpty(projectPerms.RoleEntries);
        Assert.True(projectPerms.RoleEntries.ContainsKey(roleHrib));
        Assert.True(projectPerms.RoleEntries[roleHrib].Sources.ContainsKey(TestSeedData.Project1Hrib));
        Assert.Equal(expectedPerm, projectPerms.RoleEntries[roleHrib].Sources[TestSeedData.Project1Hrib].Permission);

        // NB: Check the account permission implied by the role exists
        AssertAccountPermission(
            perms: projectPerms,
            accountHrib: TestSeedData.UserHrib,
            permission: expectedPerm,
            sourceHrib: TestSeedData.Project1Hrib,
            intermediaryRoleHrib: roleHrib
        );
    }

    [Fact]
    public async Task EntityPermissionInfo_RoleUnassignment_ShouldRemoveAccountPerms()
    {
        await EntityPermissionInfo_RoleAssignment_ShouldSetAccountPerms();

        var roleHrib = Hrib.Parse("testrole000").ToString();
        await using (var session = Store.LightweightSession())
        {
            session.Events.Append(
                TestSeedData.UserHrib,
                new AccountRoleUnset(TestSeedData.UserHrib, roleHrib)
            );
            await session.SaveChangesAsync();
        }

        await WaitForProjections();

        await using var query = Store.QuerySession();

        var membersInfo = (await query.KafeLoadAsync<RoleMembersInfo>(roleHrib)).Unwrap();
        Assert.DoesNotContain(TestSeedData.UserHrib, membersInfo.MemberIds);

        var projectPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Project1Hrib)).Unwrap();
        Assert.DoesNotContain(TestSeedData.UserHrib, projectPerms.AccountEntries.Keys);
    }

    public static readonly TheoryData<string, Type, object> CreateEvents = new() {
        {
            Hrib.Parse("createtst-o").ToString(),
            typeof(OrganizationInfo),
            new OrganizationCreated(
                Hrib.Parse("createtst-o").ToString(),
                CreationMethod.Manual,
                LocalizedString.CreateInvariant("CreateEventTest organization"))
        },
        {
            Hrib.Parse("createtst-g").ToString(),
            typeof(ProjectGroupInfo),
            new ProjectGroupCreated(
                Hrib.Parse("createtst-g").ToString(),
                CreationMethod.Manual,
                TestSeedData.Org1Hrib,
                LocalizedString.CreateInvariant("CreateEventTest project group")
            )
        },
        {
            Hrib.Parse("createtst-p").ToString(),
            typeof(ProjectInfo),
            new ProjectCreated(
                Hrib.Parse("createtst-p").ToString(),
                TestSeedData.AdminHrib,
                CreationMethod.Manual,
                TestSeedData.Group1Hrib,
                null
            )
        },
        {
            Hrib.Parse("createtst-a").ToString(),
            typeof(ArtifactInfo),
            new ArtifactCreated(
                Hrib.Parse("createtst-a").ToString(),
                CreationMethod.Manual,
                LocalizedString.CreateInvariant("CreateEventTest artifact"),
                default
            )
        },
        {
            Hrib.Parse("createtst-c").ToString(),
            typeof(AccountInfo),
            new AccountCreated(
                Hrib.Parse("createtst-c").ToString(),
                CreationMethod.Manual,
                "account@example.com",
                Kafe.Const.InvariantCultureCode
            )
        },
        {
            Hrib.Parse("createtst-u").ToString(),
            typeof(AuthorInfo),
            new AuthorCreated(
                Hrib.Parse("createtst-u").ToString(),
                CreationMethod.Manual,
                "Author Authorson"
            )
        },
        {
            Hrib.Parse("createtst-l").ToString(),
            typeof(PlaylistInfo),
            new PlaylistCreated(
                Hrib.Parse("createtst-l").ToString(),
                CreationMethod.Manual,
                TestSeedData.Org1Hrib,
                LocalizedString.CreateInvariant("CreateEventTest playlist")
            )
        },
        {
            Hrib.Parse("createtst-r").ToString(),
            typeof(RoleInfo),
            new RoleCreated(
                Hrib.Parse("createtst-r").ToString(),
                CreationMethod.Manual,
                TestSeedData.Org1Hrib,
                LocalizedString.CreateInvariant("CreateEventTest role")
            )
        }
    };

    [Theory]
    [MemberData(nameof(CreateEvents))]
    public async Task EntityPermissionInfo_CreateEvent_ShouldCreateInfo(
        string hrib,
        Type aggregateType,
        object createEvent)
    {
        await using (var session = Store.LightweightSession())
        {
            session.Events.StartStream(
                aggregateType: aggregateType,
                streamKey: hrib,
                events: [createEvent]);
            await session.SaveChangesAsync();
        }

        await WaitForProjections();
        await using var query = Store.QuerySession();
        var perms = (await query.KafeLoadAsync<EntityPermissionInfo>(hrib)).Unwrap();
        AssertAccountPermission(
            perms: perms,
            accountHrib: TestSeedData.AdminHrib,
            permission: Permission.Read | Permission.Append | Permission.Inheritable,
            sourceHrib: Hrib.System
        );
    }

    [Fact]
    public async Task EntityPermissionInfo_AccountSetPermission_ShouldSetPermission()
    {
        var testHrib = Hrib.Parse("setpermtest").ToString();
        await using (var session = Store.LightweightSession())
        {
            session.Events.StartStream<ProjectInfo>(
                testHrib,
                new ProjectCreated(
                    testHrib,
                    TestSeedData.AdminHrib,
                    CreationMethod.Manual,
                    TestSeedData.Org1Hrib,
                    null
                )
            );
            await session.SaveChangesAsync();

            session.Events.Append(
                TestSeedData.UserHrib,
                new AccountPermissionSet(
                    TestSeedData.UserHrib,
                    testHrib,
                    Permission.Read
                ));
            await session.SaveChangesAsync();
        }

        await WaitForProjections();
        await using var query = Store.QuerySession();
        var perms = (await query.KafeLoadAsync<EntityPermissionInfo>(testHrib)).Unwrap();
        AssertAccountPermission(
            perms: perms,
            accountHrib: TestSeedData.UserHrib,
            permission: Permission.Read,
            sourceHrib: testHrib
        );
    }

    [Fact]
    public async Task EntityPermissionInfo_ProjectArtifactAdded_ShouldInheritPermissions()
    {
        await using (var session = Store.LightweightSession())
        {
            session.Events.Append(
                TestSeedData.UserHrib,
                new AccountPermissionSet(
                    TestSeedData.UserHrib,
                    TestSeedData.Project1Hrib,
                    Permission.Read | Permission.Inspect | Permission.Write
                )
            );
            session.Events.Append(
                TestSeedData.Project1Hrib,
                new ProjectArtifactAdded(
                    TestSeedData.Project1Hrib,
                    TestSeedData.Artifact1Hrib,
                    null)
            );
            await session.SaveChangesAsync();
        }

        await WaitForProjections();
        await using var query = Store.QuerySession();
        var perms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Artifact1Hrib)).Unwrap();

        Assert.Equal([TestSeedData.Project1Hrib], perms.GetParents());
        Assert.Equal(
            [
                TestSeedData.Artifact1Hrib,
                TestSeedData.Project1Hrib,
                TestSeedData.Group1Hrib,
                TestSeedData.Org1Hrib,
                Hrib.SystemValue
            ],
            perms.GetGrantors());
        AssertAccountPermission(
            perms: perms,
            accountHrib: TestSeedData.UserHrib,
            permission: Permission.Read | Permission.Inspect | Permission.Append | Permission.Write,
            sourceHrib: TestSeedData.Project1Hrib
        );
    }

    [Fact]
    public async Task EntityPermissionInfo_ProjectArtifactRemoved_ShouldUninheritPermissions()
    {
        // NB: Ensure permissions can be inherited first.
        await EntityPermissionInfo_ProjectArtifactAdded_ShouldInheritPermissions();

        await using (var session = Store.LightweightSession())
        {
            session.Events.Append(
                TestSeedData.Project1Hrib,
                new ProjectArtifactRemoved(
                    TestSeedData.Project1Hrib,
                    TestSeedData.Artifact1Hrib
                )
            );
            await session.SaveChangesAsync();
        }

        await WaitForProjections();
        await using var query = Store.QuerySession();
        var perms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Artifact1Hrib)).Unwrap();
        Assert.Equal([], perms.GetParents());
        Assert.Equal([Hrib.SystemValue, TestSeedData.Artifact1Hrib], perms.GetGrantors());
        AssertAccountPermission(
            perms: perms,
            accountHrib: TestSeedData.UserHrib,
            permission: Permission.None
        );
    }

    [Fact]
    public async Task EntityPermissionInfo_ProjectGroupMovedToOrganization_ShouldReparent()
    {
        await using (var session = Store.LightweightSession())
        {
            session.Events.Append(
                TestSeedData.Group1Hrib,
                new ProjectGroupMovedToOrganization(
                    TestSeedData.Group1Hrib,
                    TestSeedData.Org2Hrib
                )
            );
            await session.SaveChangesAsync();
        }

        await WaitForProjections();
        await using var query = Store.QuerySession();
        var groupPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Group1Hrib)).Unwrap();
        Assert.Equal([TestSeedData.Org2Hrib], groupPerms.GetParents());
        Assert.Equal([TestSeedData.Org2Hrib, Hrib.SystemValue, TestSeedData.Group1Hrib], groupPerms.GetGrantors());

        var projectPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Project1Hrib)).Unwrap();
    }

    [Fact]
    public async Task EntityPermissionInfo_GlobalPermission_ShouldBeInherited()
    {
        await using (var session = Store.LightweightSession())
        {
            session.Events.Append(
                TestSeedData.Group1Hrib,
                new ProjectGroupGlobalPermissionsChanged(
                    TestSeedData.Group1Hrib,
                    GlobalPermissions: Permission.Inspect
                )
            );
            await session.SaveChangesAsync();
        }

        await WaitForProjections();
        await using var query = Store.QuerySession();
        var groupPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Group1Hrib)).Unwrap();
        Assert.Equal(Permission.Inspect, groupPerms.GlobalPermission.EffectivePermission);
        Assert.Single(groupPerms.GlobalPermission.Sources);
        Assert.True(groupPerms.GlobalPermission.Sources.ContainsKey(TestSeedData.Group1Hrib));
        Assert.Equal(Permission.Inspect, groupPerms.GlobalPermission.Sources[TestSeedData.Group1Hrib].Permission);

        var projPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(TestSeedData.Project1Hrib)).Unwrap();
        Assert.Equal(Permission.Inspect | Permission.Read, projPerms.GlobalPermission.EffectivePermission);
        Assert.True(projPerms.GlobalPermission.Sources.ContainsKey(TestSeedData.Group1Hrib));
        Assert.Equal(
            Permission.Inspect | Permission.Read,
            projPerms.GlobalPermission.Sources[TestSeedData.Group1Hrib].Permission);
    }


    // TODO: Test case: Role has Inspect on a group and Review on an org.
    //       Role must have two sources on the group: the org and the group.
    //       The effective permission is Read | Inspect | Review. All members must also have this effective permission.
    //       All accounts in it must have Read | Inspect | Review on all of the group's projects
    //       with the role as (intermediary) RoleId on the source.

    // TODO: Test case: When an artifact is referenced by multiple projects, all of those projects are parents.

    // TODO: Test case: global permission events set global permissions.

    // TODO: Test case: Using AccountPermissionSet to *remove* permissions.

    // TODO: Test case: Using RolePermissionSet to *remove* permissions.

    private static void AssertAccountPermission(
        EntityPermissionInfo perms,
        Hrib accountHrib,
        Permission permission,
        Hrib? sourceHrib = null,
        Hrib? intermediaryRoleHrib = null)
    {
        if (permission == Permission.None)
        {
            Assert.False(perms
                .AccountEntries
                .ContainsKey(accountHrib.ToString()));
            return;
        }

        Assert.NotEmpty(perms.AccountEntries);
        Assert.True(perms
            .AccountEntries
            .ContainsKey(accountHrib.ToString()));
        Assert.Equal(permission, perms
            .AccountEntries[accountHrib.ToString()]
            .EffectivePermission & permission);
        if (sourceHrib is not null)
        {
            Assert.True(perms
                .AccountEntries[accountHrib.ToString()]
                .Sources
                .ContainsKey(sourceHrib.ToString()));
            Assert.Equal(permission, perms
                .AccountEntries[accountHrib.ToString()]
                .Sources[sourceHrib.ToString()]
                .Permission);

            if (intermediaryRoleHrib is not null)
            {
                Assert.Equal(intermediaryRoleHrib, perms
                    .AccountEntries[accountHrib.ToString()]
                    .Sources[sourceHrib.ToString()]
                    .RoleId);
            }
        }
    }
}
