using System.Threading.Tasks;
using Alba;
using Kafe.Api.Transfer;
using Xunit;

namespace Kafe.Tests;

[Collection(Const.Collections.Api)]
public class AccountEndpointsTests(ApiFixture fixture) : ApiContext(fixture)
{
    [Fact]
    public async Task TemporaryAccountCreation_WithValidEmail_ShouldSucceed()
    {
        var result = await Host.Scenario(c =>
        {
            c.Post
                .Json(new TemporaryAccountCreationDto(
                    EmailAddress: "user@example.com",
                    PreferredCulture: null
                ))
                .ToUrl("/api/v1/tmp-account");
            c.StatusCodeShouldBe(200);
        });
    }
}
