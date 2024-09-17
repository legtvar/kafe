using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Documents;
using Kafe.Data.Projections;
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
}
