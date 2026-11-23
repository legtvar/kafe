using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Kafe.Tests.EndToEnd;

[Collection(FilmSubmissionEndToEndCollection.Name)]
public sealed class CoreWorkflowEndToEndTests(EndToEndFixture fixture)
{
    private static ILocator AccountMenu(IPage page) => page.GetByRole(AriaRole.Button)
        .Filter(new() { Has = page.Locator(".chakra-avatar") });
    [Fact]
    public async Task TemporaryAccount_EmailWithoutAccount_CreatesAccount()
    {
        var email = CreateTestEmail();

        await RunBrowserTestAsync("temporary-account-creation", async page =>
        {
            var account = await CreateTemporaryAccountAsync(page, email);

            Assert.NotNull(account.Id);
            Assert.Equal(email, account.EmailAddress);
            Assert.Null(account.Name);
            await AssertNoOrganizationIsAvailableAsync(page);
        });
    }

    [Fact]
    public async Task TemporaryAccount_ExistingAccount_LogsInAgain()
    {
        var email = CreateTestEmail();

        await RunBrowserTestAsync("temporary-account-login", async page =>
        {
            Account createdAccount;
            await using (var creationContext = await fixture.NewContextAsync())
            {
                var creationPage = await creationContext.NewPageAsync();
                createdAccount = await CreateTemporaryAccountAsync(creationPage, email);
            }
            await RequestTemporaryAccountAsync(page, email);
            await ConfirmTemporaryAccountAsync(page, email);
            var loggedInAccount = await ReadCurrentAccountAsync(page);

            Assert.Equal(createdAccount.Id, loggedInAccount.Id);
            Assert.Equal(createdAccount.EmailAddress, loggedInAccount.EmailAddress);
            await AssertNoOrganizationIsAvailableAsync(page);
        });
    }

    [Fact]
    public async Task ProjectGroup_Administrator_CreatesProjectGroup()
    {
        var name = $"E2E admin group {Guid.NewGuid():N}";

        await RunBrowserTestAsync("admin-project-group-creation", async page =>
        {
            await RequestTemporaryAccountAsync(page, "kafe@example.com");
            await LogInAsync(page, "kafe@example.com");
            await page.GotoAsync($"/auth/{EndToEndFixture.OrganizationId}/groups/create");

            await Expect(page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Create a project group", Exact = true }))
                .ToBeVisibleAsync();
            await page.GetByPlaceholder(
                "The name of the project group - International",
                new() { Exact = true })
                .FillAsync(name);

            var createResponse = page.WaitForResponseAsync(response =>
                response.Request.Method == "POST"
                && new Uri(response.Url).AbsolutePath == "/api/v1/project-group");
            await page.GetByRole(
                AriaRole.Button,
                new() { Name = "Create a project group", Exact = true })
                .ClickAsync();

            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)(await createResponse).Status);
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true }))
                .ToBeVisibleAsync();
            Assert.Matches(@"/groups/[A-Za-z0-9_-]+$", new Uri(page.Url).AbsolutePath);
        });
    }

    [Fact]
    public async Task GroupCreation_LocalizedLanguageTabs_AreAllReachableByKeyboard()
    {
        await RunBrowserTestAsync("localized-input-language-focus", async page =>
        {
            await RequestTemporaryAccountAsync(page, "kafe@example.com");
            await LogInAsync(page, "kafe@example.com");
            await page.GotoAsync($"/auth/{EndToEndFixture.OrganizationId}/groups/create");

            var internationalTab = page.GetByRole(AriaRole.Tab, new() { Name = "International", Exact = true }).First;
            var englishTab = page.GetByRole(AriaRole.Tab, new() { Name = "English", Exact = true }).First;
            await Expect(internationalTab).ToBeVisibleAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).First.ClickAsync();
            await page.GetByRole(AriaRole.Menuitem, new() { Name = "English", Exact = false }).First.ClickAsync();
            await Expect(englishTab).ToBeVisibleAsync();

            foreach (var tab in await page.GetByRole(AriaRole.Tab).AllAsync())
            {
                Assert.Equal("0", await tab.GetAttributeAsync("tabindex"));
            }

            await englishTab.FocusAsync();
            await page.Keyboard.PressAsync("Shift+Tab");
            await Expect(internationalTab).ToBeFocusedAsync();
            await page.Keyboard.PressAsync("Tab");
            await Expect(englishTab).ToBeFocusedAsync();
            await Expect(internationalTab).ToHaveAttributeAsync("aria-selected", "false");
            await Expect(englishTab).ToHaveAttributeAsync("aria-selected", "true");

            await page.Keyboard.PressAsync("Shift+Tab");
            await Expect(internationalTab).ToBeFocusedAsync();
            await page.Keyboard.PressAsync("Enter");
            await Expect(internationalTab).ToHaveAttributeAsync("aria-selected", "true");
            await Expect(englishTab).ToHaveAttributeAsync("aria-selected", "false");
            await page.Locator("[id='name.iv']").FillAsync("Keyboard-edited group name");
            await Expect(page.Locator("[id='name.iv']")).ToHaveValueAsync("Keyboard-edited group name");
        });
    }

    private async Task<Account> CreateTemporaryAccountAsync(IPage page, string email)
    {
        await RequestTemporaryAccountAsync(page, email);
        await ConfirmTemporaryAccountAsync(page, email);
        return await ReadCurrentAccountAsync(page);
    }

    private async Task LogInAsync(IPage page, string email)
    {
        await ConfirmTemporaryAccountAsync(page, email);
        var account = await ReadCurrentAccountAsync(page);
        Assert.Equal(email, account.EmailAddress);
        await AssertAuthenticatedUiAsync(page);
    }

    private static async Task AssertAuthenticatedUiAsync(IPage page)
    {
        var accountMenu = AccountMenu(page);
        await Expect(accountMenu).ToBeVisibleAsync();
        await accountMenu.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Menuitem, new() { Name = "Sign out", Exact = true }))
            .ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
    }

    private static async Task AssertNoOrganizationIsAvailableAsync(IPage page)
    {
        await Expect(page.GetByText("No organization available", new() { Exact = false }).First)
            .ToBeVisibleAsync();
    }

    private async Task RequestTemporaryAccountAsync(IPage page, string email)
    {
        var navigation = await page.GotoAsync("/account/login");
        Assert.NotNull(navigation);
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)navigation.Status);
        fixture.FixtureLogger.LogInformation(
            "Login page body after navigation: {Body}",
            await page.Locator("body").TextContentAsync());
        await page.Locator("input[type=email]").FillAsync(email);
        var createResponse = page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/tmp-account");
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)(await createResponse).Status);
        await Expect(page.GetByText("We have sent information", new() { Exact = false })).ToBeVisibleAsync();
    }

    private async Task ConfirmTemporaryAccountAsync(IPage page, string email)
    {
        var token = await fixture.CreateConfirmationTokenAsync(email);
        await page.GotoAsync($"/account/token/{token}");
        await page.WaitForURLAsync("**/auth");
        var acceptCookies = page.GetByRole(
            AriaRole.Button,
            new() { Name = "Yum, I'll take it!", Exact = true });
        if (await acceptCookies.CountAsync() > 0)
        {
            await acceptCookies.ClickAsync();
        }
    }

    private async Task<Account> ReadCurrentAccountAsync(IPage page)
    {
        var response = await page.Context.APIRequest.GetAsync($"{fixture.ApiBaseUrl}/api/v1/account");
        var content = await response.TextAsync();
        Assert.True(response.Ok, $"Could not read the current account: {(HttpStatusCode)response.Status} {content}");
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var name = root.TryGetProperty("name", out var nameElement)
            && nameElement.ValueKind != JsonValueKind.Null
                ? nameElement.GetString()
                : null;
        return new Account(
            root.GetProperty("id").GetString(),
            root.GetProperty("emailAddress").GetString(),
            name);
    }

    private static string CreateTestEmail() => $"e2e-{Guid.NewGuid():N}@example.test";

    private async Task RunBrowserTestAsync(string name, Func<IPage, Task> test)
    {
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        await context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        try
        {
            await test(page);
            await context.Tracing.StopAsync();
        }
        catch
        {
            Directory.CreateDirectory(fixture.ArtifactsDirectory);
            try
            {
                fixture.FixtureLogger.LogError(
                    "Browser test {Name} failed at {Url}. Page text:\n{Body}",
                    name,
                    page.Url,
                    await page.Locator("body").TextContentAsync());
                await page.ScreenshotAsync(new()
                {
                    Path = Path.Combine(fixture.ArtifactsDirectory, $"{name}.png"),
                    FullPage = true
                });
                await context.Tracing.StopAsync(new()
                {
                    Path = Path.Combine(fixture.ArtifactsDirectory, $"{name}-trace.zip")
                });
            }
            catch
            {
                // Preserve the test failure even if the browser has already crashed or disconnected.
            }

            throw;
        }
    }

    private sealed record Account(string? Id, string? EmailAddress, string? Name);
}
