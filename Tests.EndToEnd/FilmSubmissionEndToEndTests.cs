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
            var video = page.GetByTestId("video-player").Locator("video");
            await Expect(video).ToBeVisibleAsync();
            await page.GetByTestId("video-play-toggle").ClickAsync();
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
    public async Task FilmUpload_MovMimeType_ConvertsAndPlaysInBrowser()
    {
        await RunBrowserTestAsync("mov-supported", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await CreateProjectFromOpenGroupAsync(page, $"E2E-{Guid.NewGuid():N}");
            var upload = await UploadFilmAsync(page, "sample.mov");

            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(upload.ShardId));
            await WaitForVariantAsync(page, upload.ShardId!, "sd", TimeSpan.FromMinutes(3));

            await OpenFilmPreviewAsync(page);
            var video = page.GetByTestId("video-player").Locator("video");
            await Expect(video).ToBeVisibleAsync();
            await page.GetByTestId("video-play-toggle").ClickAsync();
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
            var video = page.GetByTestId("video-player").Locator("video");
            await Expect(video).ToBeVisibleAsync();
            await page.GetByTestId("video-play-toggle").ClickAsync();
            await Expect(page.GetByTestId("video-error")).ToBeVisibleAsync();
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
            await page.GetByTestId("localized-input-name-iv-input").FillAsync(title);
            await page.GetByTestId("localized-input-description-iv-input").FillAsync(description);

            await page.GetByTestId("language-selector").First.ClickAsync();
            await page.GetByTestId("language-app-option-cs").First.ClickAsync();

            await Expect(page.GetByTestId("project-create-heading")).ToHaveTextAsync("Vytvořte nový projekt");
            await Expect(page.GetByTestId("localized-input-name-iv-input")).ToHaveValueAsync(title);
            await Expect(page.GetByTestId("localized-input-description-iv-input")).ToHaveValueAsync(description);
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

            var authorId = await AddNewAuthorAsync(page, authorName, uco, czechBio, englishBio);
            await Expect(page.GetByTestId("project-author-name")).ToHaveTextAsync(authorName);

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
    public async Task ProjectAuthorLinks_ReadAndEditAuthor()
    {
        await RunBrowserTestAsync("project-author-links", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await OpenProjectCreationAsync(page);
            var authorName = $"E2E Author {Guid.NewGuid():N}";
            const string uco = "123456";
            const string czechBio = "Autor testovacího filmu.";
            const string englishBio = "Author of the test film.";
            const string updatedEnglishBio = "Author biography edited through the project.";
            var authorId = await AddNewAuthorAsync(page, authorName, uco, czechBio, englishBio);
            await FillAndSubmitProjectCreationAsync(page, $"E2E-{Guid.NewGuid():N}");
            await page.GetByTestId("project-basic-info-tab").ClickAsync();

            await page.GetByTestId("project-author-edit-details").ClickAsync();
            var editDialog = page.GetByRole(AriaRole.Dialog);
            await Expect(editDialog.GetByTestId("author-name-input")).ToHaveValueAsync(authorName);
            await Expect(editDialog.GetByTestId("author-uco-input")).ToHaveValueAsync(uco);
            await editDialog.GetByTestId("localized-input-bio-en-tab").ClickAsync();
            await Expect(editDialog.GetByTestId("localized-input-bio-en-input")).ToHaveValueAsync(englishBio);

            var updatedName = authorName + " Edited";
            await editDialog.GetByTestId("author-name-input").FillAsync(updatedName);
            await editDialog.GetByTestId("localized-input-bio-en-input").FillAsync(updatedEnglishBio);
            var editResponseTask = page.WaitForResponseAsync(response =>
                response.Request.Method == "PATCH" && new Uri(response.Url).AbsolutePath == "/api/v1/author");
            await editDialog.GetByTestId("author-save").ClickAsync();
            var editResponse = await editResponseTask;
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)editResponse.Status);
            await Expect(page.GetByTestId("project-author-name")).ToHaveTextAsync(updatedName);

            await page.GetByTestId("project-author-edit-roles").ClickAsync();
            var roleDialog = page.GetByRole(AriaRole.Dialog);
            var roleInput = roleDialog.GetByTestId("author-roles-control").Locator("input").First;
            await roleInput.FillAsync("Producer");
            await roleInput.PressAsync("ArrowDown");
            await roleInput.PressAsync("Enter");
            await Expect(roleDialog.GetByTestId("author-roles-selected-options")).ToContainTextAsync("Producer");
            await roleDialog.GetByTestId("author-roles-label").ClickAsync();
            await roleDialog.GetByTestId("author-roles-save").ClickAsync();
            await Expect(page.GetByTestId("project-author-roles")).ToHaveTextAsync("Director, Producer");
            var editPath = new Uri(page.Url).AbsolutePath;
            var projectEditResponseTask = page.WaitForResponseAsync(response =>
                response.Request.Method == "PATCH" && new Uri(response.Url).AbsolutePath == "/api/v1/project");
            var successfulReloadTask = page.WaitForResponseAsync(response =>
                response.Request.ResourceType == "document" && new Uri(response.Url).AbsolutePath == editPath);
            await page.GetByTestId("project-save").ClickAsync();
            var projectEditResponse = await projectEditResponseTask;
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)projectEditResponse.Status);
            await successfulReloadTask;
            await page.WaitForLoadStateAsync();

            await page.GetByTestId("project-basic-info-tab").ClickAsync();
            await Expect(page.GetByTestId("project-author-name")).ToHaveTextAsync(updatedName);
            await Expect(page.GetByTestId("project-author-roles")).ToHaveTextAsync("Director, Producer");

            var authorResponse = await page.Context.APIRequest.GetAsync($"{fixture.ApiBaseUrl}/api/v1/author/{authorId}");
            Assert.True(authorResponse.Ok, $"Could not inspect the edited author: {(HttpStatusCode)authorResponse.Status}");
            using var document = JsonDocument.Parse(await authorResponse.TextAsync());
            var author = document.RootElement;
            Assert.Equal(updatedName, author.GetProperty("name").GetString());
            Assert.Equal(uco, author.GetProperty("uco").GetString());
            Assert.Equal(updatedEnglishBio, author.GetProperty("bio").GetProperty("en").GetString());
            Assert.Equal(czechBio, author.GetProperty("bio").GetProperty("cs").GetString());
            Assert.Equal(JsonValueKind.Null, author.GetProperty("email").ValueKind);
            Assert.Equal(JsonValueKind.Null, author.GetProperty("phone").ValueKind);
        });
    }

    [Fact]
    public async Task FilmUpload_StatusTab_RefreshesValidationAfterAutomaticSave()
    {
        await RunBrowserTestAsync("validation-refreshed-after-upload", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await CreateProjectFromOpenGroupAsync(page, $"E2E-{Guid.NewGuid():N}");
            await Expect(page.GetByTestId("project-validation-status")).ToContainTextAsync("The film is required.");

            var upload = await UploadFilmAsync(page, "sample.mp4");
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

            var validationResponse = page.WaitForResponseAsync(response =>
                response.Request.Method == "GET"
                && new Uri(response.Url).AbsolutePath.StartsWith("/api/v1/project-validation/"));
            await page.GetByTestId("project-status-tab").ClickAsync();
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)(await validationResponse).Status);
            await Expect(page.GetByTestId("project-validation-status")).Not.ToContainTextAsync("The film is required.");
        });
    }

    [Fact]
    public async Task ArtifactDeletion_ExistingCast_PreservesCast()
    {
        await RunBrowserTestAsync("artifact-deletion-preserves-cast", async page =>
        {
            await LogInThroughTemporaryAccountAsync(page);
            await CreateProjectFromOpenGroupAsync(page, $"E2E-{Guid.NewGuid():N}");
            var projectId = GetCurrentProjectId(page);
            var castName = $"Cast {Guid.NewGuid():N}";

            var authorResponse = await page.Context.APIRequest.PostAsync(
                $"{fixture.ApiBaseUrl}/api/v1/author",
                new()
                {
                    DataObject = new
                    {
                        name = castName,
                        globalPermissions = "read",
                        bio = (string?)null,
                        uco = (string?)null,
                        email = (string?)null,
                        phone = (string?)null
                    }
                });
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)authorResponse.Status);
            var authorId = (await authorResponse.TextAsync()).Trim().Trim('"');

            var castResponse = await page.Context.APIRequest.PatchAsync(
                $"{fixture.ApiBaseUrl}/api/v1/project",
                new()
                {
                    DataObject = new
                    {
                        id = projectId,
                        cast = new[] { new { id = authorId, roles = new[] { "Actor" } } }
                    }
                });
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)castResponse.Status);
            Assert.Equal(castName, GetSingleCastName(await GetProjectDocumentAsync(page, projectId)));

            var upload = await UploadFilmAsync(page, "sample.mp4");
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
            await page.ReloadAsync();
            await OpenFilesTabAsync(page);
            var filmSection = page.GetByTestId("artifact-group-film");
            await filmSection.GetByTestId("artifact-film-toggle").ClickAsync();
            await filmSection.GetByTestId("artifact-delete").ClickAsync();

            var deletionResponseTask = page.WaitForResponseAsync(response =>
                response.Request.Method == "PATCH" && new Uri(response.Url).AbsolutePath == "/api/v1/project");
            var confirmation = page.Locator("[role='alertdialog']");
            await confirmation.GetByTestId("artifact-delete-confirm").ClickAsync();
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)(await deletionResponseTask).Status);

            using var project = await GetProjectDocumentAsync(page, projectId);
            Assert.Equal(castName, GetSingleCastName(project));
        });
    }

    [Fact]
    public async Task ProjectGroup_AnonymousVisitor_RedirectsToLogin()
    {
        await RunBrowserTestAsync("authentication-required", async page =>
        {
            await page.GotoAsync(fixture.GroupPath);
            await page.WaitForURLAsync("**/account/login**");
            await Expect(page.GetByTestId("account-login-heading")).ToBeVisibleAsync();
        });
    }

    private static async Task<string> AddNewAuthorAsync(
        IPage page,
        string authorName,
        string uco,
        string czechBio,
        string englishBio)
    {
        await page.GetByTestId("author-add-button").First.ClickAsync();
        var dialog = page.GetByRole(AriaRole.Dialog);
        await dialog.GetByTestId("author-name-query").FillAsync(authorName);
        await dialog.GetByTestId("author-create-option").ClickAsync();
        await dialog.GetByTestId("author-uco-input").FillAsync(uco);
        await dialog.GetByTestId("localized-input-bio-cs-tab").ClickAsync();
        await dialog.GetByTestId("localized-input-bio-cs-input").FillAsync(czechBio);
        await dialog.GetByTestId("localized-input-bio-en-tab").ClickAsync();
        await dialog.GetByTestId("localized-input-bio-en-input").FillAsync(englishBio);

        var createResponseTask = page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/author");
        await dialog.GetByTestId("author-save").ClickAsync();
        var createResponse = await createResponseTask;
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)createResponse.Status);
        var authorId = (await createResponse.TextAsync()).Trim().Trim('"');
        var roleInput = dialog.GetByTestId("author-roles-control").Locator("input").First;
        await roleInput.FillAsync("Director");
        await dialog.GetByRole(AriaRole.Option).Nth(2).ClickAsync();
        await Expect(dialog.GetByTestId("author-roles-selected-options")).ToContainTextAsync("Director");
        await dialog.GetByTestId("author-roles-label").ClickAsync();
        await dialog.GetByTestId("author-roles-save").ClickAsync();
        await Expect(page.GetByTestId("project-author-name")).ToHaveTextAsync(authorName);
        await Expect(dialog).Not.ToBeVisibleAsync();
        return authorId;
    }

    private async Task LogInThroughTemporaryAccountAsync(IPage page)
    {
        const string email = "kafe@example.com";
        await page.GotoAsync("/account/login");
        await page.GetByTestId("account-login-email").FillAsync(email);
        await page.GetByTestId("account-login-submit").ClickAsync();
        await Expect(page.GetByTestId("account-login-confirmation-sent")).ToBeVisibleAsync();

        var token = await fixture.CreateConfirmationTokenAsync(email);
        await page.GotoAsync($"/account/token/{token}");
        await page.WaitForURLAsync("**/auth");
        var acceptCookies = page.GetByTestId("cookie-consent-accept");
        if (await acceptCookies.CountAsync() > 0)
        {
            await acceptCookies.ClickAsync();
        }
    }

    private async Task CreateProjectFromOpenGroupAsync(IPage page, string title)
    {
        await OpenProjectCreationAsync(page);
        await FillAndSubmitProjectCreationAsync(page, title);
    }

    private static async Task FillAndSubmitProjectCreationAsync(IPage page, string title)
    {
        await page.GetByTestId("localized-input-name-iv-input").FillAsync(title);
        await page.GetByTestId("localized-input-name-en-input").FillAsync(title);
        await page.GetByTestId("localized-input-name-cs-input").FillAsync(title);
        await page.GetByTestId("localized-input-description-iv-input")
            .FillAsync("End-to-end browser upload of a small film festival sample video.");
        await page.GetByTestId("localized-input-description-en-input")
            .FillAsync("End-to-end browser upload of a small film festival sample video.");
        await page.GetByTestId("localized-input-description-cs-input")
            .FillAsync("End-to-end browser upload of a small film festival sample video.");

        var genreInput = page.GetByTestId("project-genre-control").Locator("input").First;
        await genreInput.FillAsync("Animation");
        await genreInput.PressAsync("ArrowDown");
        await genreInput.PressAsync("Enter");
        await Expect(page.GetByTestId("project-genre-selected-options")).ToContainTextAsync("Animation");

        var createResponse = page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/project");
        await page.GetByTestId("project-submit").ClickAsync();
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)(await createResponse).Status);
        await page.WaitForURLAsync("**/projects/*/edit");
    }

    private async Task OpenProjectCreationAsync(IPage page)
    {
        await page.GotoAsync("/auth/test----org/groups/test--group");
        var createProjectButton = page.GetByTestId("project-create-button");
        var editGroupButton = page.GetByTestId("project-group-edit-button");
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
        await filmSection.GetByTestId("artifact-create-film").ClickAsync();
        await filmSection.GetByTestId("artifact-film-toggle").ClickAsync();
        await filmSection.GetByTestId("shard-video-add-file").First.ClickAsync();

        var uploadResponse = page.WaitForResponseAsync(response =>
            response.Request.Method == "POST" && new Uri(response.Url).AbsolutePath == "/api/v1/shard");
        var mimeType = Path.GetExtension(fileName) switch
        {
            ".mkv" => "video/x-matroska",
            ".mov" => "video/quicktime",
            _ => "video/mp4"
        };
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
        await filmSection.GetByTestId("shard-video-input").First.SetInputFilesAsync(new[]
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
            await Expect(filmSection.GetByTestId("shard-video-file-name").First)
                .ToHaveTextAsync(Path.GetFileName(fileName));
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

    private static string GetCurrentProjectId(IPage page)
    {
        return page.Url.Split("/projects/")[1].Split('/')[0];
    }

    private async Task<JsonDocument> GetProjectDocumentAsync(IPage page, string projectId)
    {
        var response = await page.Context.APIRequest.GetAsync($"{fixture.ApiBaseUrl}/api/v1/project/{projectId}");
        Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.Status);
        return JsonDocument.Parse(await response.TextAsync());
    }

    private static string GetSingleCastName(JsonDocument project)
    {
        var cast = project.RootElement.GetProperty("cast").EnumerateArray().ToArray();
        return Assert.Single(cast).GetProperty("name").GetString()!;
    }

    private static async Task OpenFilmPreviewAsync(IPage page)
    {
        await page.ReloadAsync();
        await OpenFilesTabAsync(page);
        var filmSection = page.GetByTestId("artifact-group-film");
        await filmSection.GetByTestId("artifact-film-toggle").ClickAsync();
        await filmSection.GetByTestId("artifact-preview").ClickAsync();
    }

    private static async Task OpenFilesTabAsync(IPage page)
    {
        var tab = page.GetByTestId("project-files-tab");
        await Expect(tab).ToBeVisibleAsync();
        await tab.ClickAsync();
    }

    private static async Task<ILocator> OpenFilmSectionForArtifactCreationAsync(IPage page)
    {
        var filmSection = page.GetByTestId("artifact-group-film");
        await Expect(filmSection).ToBeVisibleAsync();
        return filmSection;
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
