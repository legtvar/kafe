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
        Assert.False(systemPerms.HasErrors);
        Assert.NotNull(systemPerms.Value);
    }

    [Fact]
    public async Task EntityPermissionInfo_System_ShouldHaveAdmin()
    {
        await WaitForProjections();
        await using var query = Store.QuerySession();
        var systemPerms = (await query.KafeLoadAsync<EntityPermissionInfo>(Hrib.System)).Unwrap();
        Assert.True(systemPerms.AccountEntries.ContainsKey(TestSeedData.AdminHrib));
        Assert.True(systemPerms
            .AccountEntries[TestSeedData.AdminHrib]
            .Sources
            .ContainsKey(Hrib.System.ToString()));
        Assert.Equal(Permission.All, systemPerms
            .AccountEntries[TestSeedData.AdminHrib]
            .Sources[Hrib.System.ToString()]
            .Permission);
        Assert.Equal(Permission.All, systemPerms
            .AccountEntries[TestSeedData.AdminHrib]
            .EffectivePermission);
    }

    [Fact]
    public async Task EntityPermissionInfo_All_ShouldHaveAdmin()
    {
        await WaitForProjections();
        await using var query = Store.QuerySession();
        var allPerms = query.Query<EntityPermissionInfo>().ToAsyncEnumerable();
        await foreach (var perms in allPerms)
        {
            Assert.True(perms.AccountEntries.ContainsKey(TestSeedData.AdminHrib));
            Assert.True(perms
                .AccountEntries[TestSeedData.AdminHrib]
                .Sources
                .ContainsKey(Hrib.System.ToString()));
            var expected = perms.Id == Hrib.System
                ? Permission.All
                : Permission.Read | Permission.Inheritable;
            Assert.Equal(expected, perms
                .AccountEntries[TestSeedData.AdminHrib]
                .Sources[Hrib.System.ToString()]
                .Permission);
            Assert.Equal(expected, perms
                .AccountEntries[TestSeedData.AdminHrib]
                .EffectivePermission & expected);
        }
    }

    [Fact]
    public async Task EntityPermissionInfo_RoleAssignment_ShouldSetAccountPerms()
    {
        var roleHrib = Hrib.Parse("testrole000").Unwrap().ToString();
        var expectedPerm = Permission.Read | Permission.Write | Permission.Administer;
        await using (var session = Store.LightweightSession())
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

        await WaitForProjections();

        await using var query = Store.QuerySession();
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

    public static readonly TheoryData<string, Type, object> CreateEvents = new() {
        {
            Hrib.Parse("createtst-o").Unwrap().ToString(),
            typeof(OrganizationInfo),
            new OrganizationCreated(
                Hrib.Parse("createtst-o").Unwrap().ToString(),
                CreationMethod.Manual,
                LocalizedString.CreateInvariant("CreateEventTest organization"))
        },
        {
            Hrib.Parse("createtst-g").Unwrap().ToString(),
            typeof(ProjectGroupInfo),
            new ProjectGroupCreated(
                Hrib.Parse("createtst-g").Unwrap().ToString(),
                CreationMethod.Manual,
                TestSeedData.TestOrganizationHrib,
                LocalizedString.CreateInvariant("CreateEventTest project group")
            )
        },
        {
            Hrib.Parse("createtst-p").Unwrap().ToString(),
            typeof(ProjectInfo),
            new ProjectCreated(
                Hrib.Parse("createtst-p").Unwrap().ToString(),
                CreationMethod.Manual,
                TestSeedData.TestGroupHrib,
                LocalizedString.CreateInvariant("CreateEventTest project")
            )
        },
        {
            Hrib.Parse("createtst-a").Unwrap().ToString(),
            typeof(ArtifactInfo),
            new ArtifactCreated(
                Hrib.Parse("createtst-a").Unwrap().ToString(),
                CreationMethod.Manual,
                LocalizedString.CreateInvariant("CreateEventTest artifact"),
                default
            )
        },
        {
            Hrib.Parse("createtst-c").Unwrap().ToString(),
            typeof(AccountInfo),
            new AccountCreated(
                Hrib.Parse("createtst-c").Unwrap().ToString(),
                CreationMethod.Manual,
                "account@example.com",
                Kafe.Const.InvariantCultureCode
            )
        },
        {
            Hrib.Parse("createtst-u").Unwrap().ToString(),
            typeof(AuthorInfo),
            new AuthorCreated(
                Hrib.Parse("createtst-u").Unwrap().ToString(),
                CreationMethod.Manual,
                "Author Authorson"
            )
        },
        {
            Hrib.Parse("createtst-l").Unwrap().ToString(),
            typeof(PlaylistInfo),
            new PlaylistCreated(
                Hrib.Parse("createtst-l").Unwrap().ToString(),
                CreationMethod.Manual,
                TestSeedData.TestOrganizationHrib,
                LocalizedString.CreateInvariant("CreateEventTest playlist")
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
        Assert.NotEmpty(perms.AccountEntries);
        Assert.True(perms.AccountEntries.ContainsKey(TestSeedData.AdminHrib));
        Assert.Equal(
            Permission.Read | Permission.Inheritable,
            perms.AccountEntries[TestSeedData.AdminHrib].EffectivePermission);
    }

    // TODO: Test case: Role has Inspect on a group and Review on an org.
    //       Role must have two sources on the group: the org and the group.
    //       The effective permission is Read | Inspect | Review. All members must also have this effective permission.
    //       All accounts in it must have Read | Inspect | Review on all of the group's projects
    //       with the role as source.
}
