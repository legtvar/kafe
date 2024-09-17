using System.Threading.Tasks;
using Xunit;

namespace Kafe.Tests;

public class PermissionTests(ApiFixture fixture) : ApiContext(fixture)
{
    [Fact]
    public async Task RoleAssignment_ShouldGiveAccountPermission()
    {
        
    }
}
