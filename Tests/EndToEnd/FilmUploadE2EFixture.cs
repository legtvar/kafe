using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Playwright;
using Xunit;

namespace Kafe.Tests.EndToEnd;

public sealed class FilmUploadE2EFixture : IAsyncLifetime
{
    public const string GroupId = "test--group";
    public const string OrganizationId = "test----org";
    public const string SkipMessage = "Set KAFE_E2E=1 to provision the containerized E2E stack.";

    private ComposeContainer? stack;
    private IContainer? api;
    private IPlaywright? playwright;
    private string? repositoryRoot;

    public IBrowser? Browser { get; private set; }
    public bool IsConfigured => Environment.GetEnvironmentVariable("KAFE_E2E") == "1";
    public string BaseUrl => "https://localhost:3001";
    public string ApiBaseUrl => "https://localhost:44369";
    public string GroupPath => $"/auth/{OrganizationId}/groups/{GroupId}";
    public string ArtifactsDirectory => Environment.GetEnvironmentVariable("KAFE_E2E_ARTIFACTS")
        ?? Path.Combine(RepositoryRoot, "Tests", "EndToEnd", "artifacts");

    private string RepositoryRoot => repositoryRoot ??=
        Environment.GetEnvironmentVariable("KAFE_REPOSITORY_ROOT") ?? FindRepositoryRoot();

    public async Task InitializeAsync()
    {
        if (!IsConfigured)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(ArtifactsDirectory);
            var composeOverride = Path.Combine(ArtifactsDirectory, "docker-compose.override.yml");
            await File.WriteAllTextAsync(composeOverride,
                $"""
                services:
                  blender:
                    build:
                      context: '{Path.GetDirectoryName(composeOverride)}'
                      dockerfile: Dockerfile.blender
                  web:
                    build:
                      target: builder
                    command: ["pnpm", "run", "start"]
                    ports:
                      - "3001:3000"
                  db:
                    healthcheck:
                      test: ["CMD-SHELL", "pg_isready -U postgres -d postgres"]
                      interval: 2s
                      timeout: 3s
                      retries: 30
                  api:
                    depends_on:
                      db:
                        condition: service_healthy
                    environment:
                      FrontEndUrl: https://localhost:3001
                      VideoConversion__PollWaitTime: 00:00:01
                """);
            await File.WriteAllTextAsync(
                Path.Combine(ArtifactsDirectory, "Dockerfile.blender"),
                "FROM alpine:3.23\nCMD [\"sleep\", \"infinity\"]\n");

            // docker-compose.base.yml uses this external network for its Traefik alias.
            var backbone = new NetworkBuilder().WithName("backbone").WithReuse(true).Build();
            await backbone.CreateAsync();

            stack = new ComposeBuilder("docker:29-cli")
                .WithEnvironment("COMPOSE_PARALLEL_LIMIT", "1")
                .WithComposeFile(
                    Path.Combine(RepositoryRoot, "docker-compose.base.yml"),
                    Path.Combine(RepositoryRoot, "docker-compose.local.yml"),
                    composeOverride)
                .WithProjectNamePrefix("kafe-e2e-")
                .WithService("db", "api", "web")
                .WithExposedService("api", 44369)
                .WithExposedService("web", 3000)
                .Build();
            await stack.StartAsync();

            api = stack.GetServiceContainer("api");
            await api.ExecAsync([
                "mkdir", "-p",
                "/mnt/kafe_generated/videos",
                "/mnt/kafe_generated/images",
                "/mnt/kafe_generated/subtitles",
                "/mnt/kafe_generated/blends",
                "/mnt/kafe_generated/unknown"
            ]);

            playwright = await Playwright.CreateAsync();
            Browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true,
                Channel = "chromium",
                Args = ["--no-sandbox", "--autoplay-policy=no-user-gesture-required"]
            });
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task<IBrowserContext> NewContextAsync()
    {
        if (Browser is null)
        {
            throw new InvalidOperationException("The E2E browser has not been initialized.");
        }

        var context = await Browser.NewContextAsync(new()
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true,
            Locale = "en-US",
            ViewportSize = new() { Width = 1440, Height = 1000 }
        });
        context.SetDefaultTimeout(30_000);
        return context;
    }

    public async Task<string> CreateConfirmationTokenAsync(string email, TimeSpan? timeout = null)
    {
        if (api is null)
        {
            throw new InvalidOperationException("The E2E API has not been initialized.");
        }

        var stopwatch = Stopwatch.StartNew();
        var maximumWait = timeout ?? TimeSpan.FromSeconds(30);
        var emailPattern = new Regex(
            $@"To:\s*{Regex.Escape(email)}[\s\S]*?/account/token/(?<token>[A-Za-z0-9_-]+)",
            RegexOptions.Compiled);

        while (stopwatch.Elapsed < maximumWait)
        {
            var logs = await api.GetLogsAsync(timestampsEnabled: false);
            var matches = emailPattern.Matches(logs.Stdout + logs.Stderr);
            if (matches.Count > 0)
            {
                return matches[^1].Groups["token"].Value;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"No confirmation email for '{email}' appeared in the API logs within {maximumWait}.");
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        playwright?.Dispose();
        if (stack is not null)
        {
            await stack.DisposeAsync();
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "docker-compose.base.yml")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the KAFE repository root.");
    }
}
