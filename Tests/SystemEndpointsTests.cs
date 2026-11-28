using System.Security.Claims;
using System.Threading.Tasks;
using Alba;
using Kafe.Api.Transfer;
using Xunit;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public sealed class SystemEndpointsTests(ApiFixture fixture, ITestOutputHelper testOutput)
    : ApiTestBase(fixture, testOutput)
{
    [Fact]
    public async Task VideoConversionStats_EmptyDatabase_ReturnsZeros()
    {
        var result = await Host.Scenario(c =>
        {
            c.WithClaim(new Claim(ClaimTypes.Email, TestSeedData.AdminEmail));
            c.WithClaim(new Claim(ClaimTypes.NameIdentifier, TestSeedData.AdminHrib));
            c.Get.Url("/api/v1/system/video-conversion-stats");
            c.StatusCodeShouldBe(200);
        });

        var stats = await result.ReadAsJsonAsync<VideoConversionStatsDto>();
        Assert.Equal(new VideoConversionStatsDto(0, 0, 0, 0), stats);
    }
}
