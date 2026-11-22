using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Kafe.Tests.EndToEnd;

[Collection(FilmSubmissionEndToEndCollection.Name)]
public sealed class FilmSubmissionEndToEndTests(EndToEndFixture fixture)
{
    [Theory]
    [InlineData("sample.mp4")]
    [InlineData("sample.mkv")]
    public async Task FilmUpload_ValidMp4OrMkvFile_ConvertsAndPlaysInBrowser(string fileName)
    {
        await RunBrowserTestAsync($"happy-{fileName}", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await CreateProjectFromOpenGroupAsync(page, $"E2E-{Guid.NewGuid():N}");
            var upload = await UploadFilmAsync(page, fileName);

            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(upload.ShardId));
            await WaitForVariantAsync(page, upload.ShardId!, "sd", TimeSpan.FromMinutes(3));

            await OpenFilmPreviewAsync(page);
            var video = page.Locator("video");
            await Expect(video).ToBeVisibleAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Play", Exact = true }).ClickAsync();
            await page.WaitForFunctionAsync(
                """
                () => {
                    const video = document.querySelector('video');
                    return video && !video.paused && video.readyState >= 2 && video.currentTime > 0.1;
                }
                """);

            var source = await video.GetAttributeAsync("src");
            Assert.Contains("/sd", source);
            Assert.True(await video.EvaluateAsync<double>("video => video.currentTime") > 0.1);
        });
    }

    [Fact]
    public async Task FilmUpload_MovMimeType_RejectsUploadAndAllowsRetry()
    {
        await RunBrowserTestAsync("mov-rejected", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await CreateProjectFromOpenGroupAsync(page, $"E2E-{Guid.NewGuid():N}");
            var upload = await UploadFilmAsync(page, "sample.mov");

            Assert.True((int)upload.StatusCode >= 400, $"MOV unexpectedly returned {(int)upload.StatusCode}.");
            await Expect(page.GetByText("Upload failed", new() { Exact = true })).ToBeVisibleAsync();

            // The error state is clickable and returns to a usable upload control.
            await page.GetByText("Upload failed", new() { Exact = true }).ClickAsync();
            await Expect(page.Locator("input[type=file]").First).ToBeAttachedAsync();
        });
    }

    [Fact]
    public async Task FilmUpload_CorruptVideo_DoesNotReachPlayableState()
    {
        await RunBrowserTestAsync("corrupt-video", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await CreateProjectFromOpenGroupAsync(page, $"E2E-{Guid.NewGuid():N}");
            var upload = await UploadFilmAsync(page, "corrupt.mp4");

            // KAFE stores the original and records failed media analysis; it must not offer a playable video.
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
            await OpenFilmPreviewAsync(page);
            var video = page.Locator("video");
            await Expect(video).ToBeVisibleAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Play", Exact = true }).ClickAsync();
            await Expect(page.GetByText(
                "Unfortunately, this video cannot be played in this quality",
                new() { Exact = true })).ToBeVisibleAsync();
            Assert.Equal(0, await video.EvaluateAsync<int>("video => video.readyState"));
            Assert.True(await video.EvaluateAsync<bool>("video => video.paused"));
        });
    }

    [Fact]
    public async Task ProjectCreation_LanguageSwitch_PreservesUnsavedProgress()
    {
        await RunBrowserTestAsync("project-creation-language-switch", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await OpenProjectCreationAsync(page);
            var title = $"E2E-{Guid.NewGuid():N}";
            const string description = "This draft must survive an interface language switch.";
            await page.Locator("[id='name.iv']").FillAsync(title);
            await page.Locator("[id='description.iv']").FillAsync(description);

            await page.GetByRole(AriaRole.Button, new() { Name = "Language", Exact = true }).ClickAsync();
            await page.GetByRole(AriaRole.Menuitemradio, new() { Name = "Czech", Exact = false }).ClickAsync();

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Vytvořte nový projekt", Exact = true }))
                .ToBeVisibleAsync();
            await Expect(page.Locator("[id='name.iv']")).ToHaveValueAsync(title);
            await Expect(page.Locator("[id='description.iv']")).ToHaveValueAsync(description);
        });
    }

    [Fact]
    public async Task AuthorCreation_LocalizedBio_PersistsAndLeavesContactsEmpty()
    {
        await RunBrowserTestAsync("author-localized-bio", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await OpenProjectCreationAsync(page);
            var authorName = $"E2E Author {Guid.NewGuid():N}";
            const string uco = "123456";
            const string czechBio = "Autor testovacího filmu.";
            const string englishBio = "Author of the test film.";

            await page.GetByRole(AriaRole.Button, new() { Name = "Add a person", Exact = true }).First.ClickAsync();
            var dialog = page.GetByRole(AriaRole.Dialog);
            await dialog.GetByPlaceholder("Name", new() { Exact = true }).First.FillAsync(authorName);
            await dialog.GetByText("New author", new() { Exact = true }).ClickAsync();
            await dialog.GetByPlaceholder("UCO", new() { Exact = true }).FillAsync(uco);
            await dialog.Locator("[id='bio.cs']").FillAsync(czechBio);
            await dialog.Locator("[id='bio.en']").FillAsync(englishBio);

            var createResponseTask = page.WaitForResponseAsync(response =>
                response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/author");
            await dialog.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).Last.ClickAsync();
            var createResponse = await createResponseTask;
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)createResponse.Status);
            var authorId = (await createResponse.TextAsync()).Trim().Trim('"');

            await dialog.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();
            await Expect(page.GetByText(authorName, new() { Exact = true })).ToBeVisibleAsync();
            await Expect(dialog).Not.ToBeVisibleAsync();

            var authorResponse = await page.Context.APIRequest.GetAsync($"{fixture.ApiBaseUrl}/api/v1/author/{authorId}");
            Assert.True(authorResponse.Ok, $"Could not inspect the new author: {(HttpStatusCode)authorResponse.Status}");
            using var document = JsonDocument.Parse(await authorResponse.TextAsync());
            var author = document.RootElement;
            Assert.Equal(uco, author.GetProperty("uco").GetString());
            Assert.Equal(JsonValueKind.Null, author.GetProperty("email").ValueKind);
            Assert.Equal(JsonValueKind.Null, author.GetProperty("phone").ValueKind);
            Assert.Equal(czechBio, author.GetProperty("bio").GetProperty("cs").GetString());
            Assert.Equal(englishBio, author.GetProperty("bio").GetProperty("en").GetString());
        });
    }

    [Fact]
    public async Task ProjectGroup_AnonymousVisitor_RedirectsToLogin()
    {
        await RunBrowserTestAsync("authentication-required", async page =>
        {
            await page.GotoAsync(fixture.GroupPath);
            await page.WaitForURLAsync("**/account/login**");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Log in to KAFE", Exact = true }))
                .ToBeVisibleAsync();
        });
    }

    private async Task LogInThroughTemporaryAccountAsync(IPage page)
    {
        const string email = "kafe@example.com";
        await page.GotoAsync("/account/login");
        await page.Locator("input[type=email]").FillAsync(email);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        await Expect(page.GetByText("We have sent information", new() { Exact = false })).ToBeVisibleAsync();

        var token = await fixture.CreateConfirmationTokenAsync(email);
        await page.GotoAsync($"/account/token/{token}");
        await page.WaitForURLAsync("**/auth");
        var acceptCookies = page.GetByRole(AriaRole.Button, new() { Name = "Yum, I'll take it!", Exact = true });
        if (await acceptCookies.CountAsync() > 0)
        {
            await acceptCookies.ClickAsync();
        }
    }

    private async Task CreateProjectFromOpenGroupAsync(IPage page, string title)
    {
        await OpenProjectCreationAsync(page);
        await page.Locator("[id='name.iv']").FillAsync(title);
        await page.Locator("[id='name.en']").FillAsync(title);
        await page.Locator("[id='name.cs']").FillAsync(title);
        await page.Locator("[id='description.iv']")
            .FillAsync("End-to-end browser upload of a small film festival sample video.");
        await page.Locator("[id='description.en']")
            .FillAsync("End-to-end browser upload of a small film festival sample video.");
        await page.Locator("[id='description.cs']")
            .FillAsync("End-to-end browser upload of a small film festival sample video.");

        var genreControl = page.GetByText("Genre", new() { Exact = true }).Locator("..");
        await genreControl.Locator("input").First.FillAsync("Animation");
        await page.GetByRole(AriaRole.Option, new() { Name = "Animation", Exact = true }).ClickAsync();

        var createResponse = page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/project");
        await page.GetByRole(AriaRole.Button, new() { Name = "Create project", Exact = true }).ClickAsync();
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)(await createResponse).Status);
        await page.WaitForURLAsync("**/projects/*/edit");
    }

    private async Task OpenProjectCreationAsync(IPage page)
    {
        await page.GotoAsync("/auth/test----org/groups/test--group");
        var createProjectButton = page.GetByRole(AriaRole.Button, new() { Name = "Create a project", Exact = true });
        var editGroupButton = page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true });
        await Expect(editGroupButton).ToBeVisibleAsync();
        if (await createProjectButton.CountAsync() == 0)
        {
            var response = await page.Context.APIRequest.PatchAsync(
                $"{fixture.ApiBaseUrl}/api/v1/project-group",
                new() { DataObject = new { id = "test--group", isOpen = true } });
            Assert.True(response.Ok, $"Could not open the E2E project group: {(HttpStatusCode)response.Status} {await response.TextAsync()}");
            await page.ReloadAsync();
        }

        await createProjectButton.ClickAsync();
    }

    private static async Task<UploadResult> UploadFilmAsync(IPage page, string fileName)
    {
        await OpenFilesTabAsync(page);
        var filmSection = await OpenFilmSectionForArtifactCreationAsync(page);
        await filmSection.GetByText("Create a new artifact", new() { Exact = true }).ClickAsync();
        await filmSection.GetByRole(AriaRole.Button, new() { Name = "Film", Exact = true }).ClickAsync();
        await filmSection.GetByText("Přidat nový soubor", new() { Exact = true }).First.ClickAsync();

        var uploadResponse = page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/shard");
        var mimeType = Path.GetExtension(fileName) switch
        {
            ".mkv" => "video/x-matroska",
            ".mov" => "video/quicktime",
            _ => "video/mp4"
        };
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
        await filmSection.Locator("input[type=file]").First.SetInputFilesAsync(new[]
        {
            new FilePayload
            {
                Name = fileName,
                MimeType = mimeType,
                Buffer = await File.ReadAllBytesAsync(fixturePath)
            }
        });
        var response = await uploadResponse;
        var statusCode = (HttpStatusCode)response.Status;
        string? shardId = null;
        if (statusCode == HttpStatusCode.OK)
        {
            shardId = (await response.TextAsync()).Trim().Trim('"');
            await Expect(filmSection.GetByText(Path.GetFileName(fileName), new() { Exact = true })).ToBeVisibleAsync();
        }

        return new(statusCode, shardId);
    }

    private async Task WaitForVariantAsync(IPage page, string shardId, string variant, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            var response = await page.Context.APIRequest.GetAsync($"{fixture.ApiBaseUrl}/api/v1/shard/{shardId}");
            Assert.True(response.Ok, $"Could not inspect video shard: {(HttpStatusCode)response.Status} {await response.TextAsync()}");
            using var document = JsonDocument.Parse(await response.TextAsync());
            if (document.RootElement.GetProperty("variants").TryGetProperty(variant, out _))
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Video '{shardId}' did not acquire the '{variant}' variant within {timeout}.");
    }

    private static async Task OpenFilmPreviewAsync(IPage page)
    {
        await page.ReloadAsync();
        await OpenFilesTabAsync(page);
        var filmLabel = page.Locator("label").GetByText("Film", new() { Exact = true });
        var filmSection = filmLabel.Locator("..");
        await filmSection.GetByRole(AriaRole.Button, new() { Name = "Film", Exact = true }).ClickAsync();
        await filmSection.GetByText("Preview", new() { Exact = true })
            .Locator("xpath=following-sibling::*[1]").ClickAsync();
    }

    private static async Task OpenFilesTabAsync(IPage page)
    {
        var tab = page.GetByRole(AriaRole.Tab, new() { Name = "Files", Exact = true });
        await Expect(tab).ToBeVisibleAsync();
        await tab.ClickAsync();
    }

    private static async Task<ILocator> OpenFilmSectionForArtifactCreationAsync(IPage page)
    {
        var filmLabel = page.Locator("label").GetByText("Film", new() { Exact = true });
        await Expect(filmLabel).ToBeVisibleAsync();
        return filmLabel.Locator("..");
    }

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
            var safeName = name.Replace("/", "-");
            Directory.CreateDirectory(fixture.ArtifactsDirectory);
            try
            {
                await page.ScreenshotAsync(new()
                {
                    Path = Path.Combine(fixture.ArtifactsDirectory, $"{safeName}.png"),
                    FullPage = true
                });
                await context.Tracing.StopAsync(new()
                {
                    Path = Path.Combine(fixture.ArtifactsDirectory, $"{safeName}-trace.zip")
                });
            }
            catch
            {
                // Preserve the test failure even if the browser has already crashed or disconnected.
            }

            throw;
        }
    }

    private sealed record UploadResult(HttpStatusCode StatusCode, string? ShardId);
}
