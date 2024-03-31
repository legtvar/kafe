using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Alba;
using Kafe.Api.Transfer;
using Kafe.Data;
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

    // TODO: Test checking that a confirmation email has been sent.

    [Fact]
    public async Task TemporaryAccountCreation_WithNullEmail_ShouldFail()
    {
        var result = await Host.Scenario(c =>
        {
            c.Post
                .Json(new TemporaryAccountCreationDto(
                    EmailAddress: null!,
                    PreferredCulture: null
                ))
                .ToUrl("/api/v1/tmp-account");
            c.StatusCodeShouldBe(400);
        });
    }

    [Fact]
    public async Task TemporaryAccountCreation_WithInvalidEmail_ShouldFail()
    {
        var result = await Host.Scenario(c =>
        {
            c.Post
                .Json(new TemporaryAccountCreationDto(
                    EmailAddress: "!@#$%^&*()",
                    PreferredCulture: null
                ))
                .ToUrl("/api/v1/tmp-account");
            c.StatusCodeShouldBe(400);
        });
    }

    [Fact]
    public async Task AccountDetailEndpoint_AnonCheckingAny_ShouldBeUnauthorized()
    {
        var result = await Host.Scenario(c =>
        {
            c.Get.Url($"/api/v1/account/{TestSeedData.UserHrib}");
            c.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
        });
    }

    [Fact]
    public async Task AccountDetailEndpoint_AdminCheckingItself_ShouldSucceed()
    {
        var result = await Host.Scenario(c =>
        {
            c.WithClaim(new Claim(ClaimTypes.Email, TestSeedData.AdminEmail));
            c.WithClaim(new Claim(ClaimTypes.NameIdentifier, TestSeedData.AdminHrib));

            c.Get.Url($"/api/v1/account/{TestSeedData.AdminHrib}");
            c.StatusCodeShouldBe(200);
        });

        var account = await result.ReadAsJsonAsync<AccountDetailDto>();
        Assert.NotNull(account);
        Assert.Equal(TestSeedData.AdminEmail, account.EmailAddress);
        Assert.Contains(Hrib.System, account.Permissions.Keys);
        Assert.Equivalent(
            TransferMaps.ToPermissionArray(Permission.All),
            account.Permissions.GetValueOrDefault(Hrib.System));
    }
}
