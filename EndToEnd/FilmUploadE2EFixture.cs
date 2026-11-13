using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Playwright;
using Xunit;

namespace Kafe.EndToEnd;

public sealed class FilmUploadE2EFixture : IAsyncLifetime
{
    public const string GroupId = "test--group";
    public const string OrganizationId = "test----org";

    private readonly List<IFutureDockerImage> images = [];
    private ComposeContainer? stack;
    private IContainer? api;
    private IPlaywright? playwright;
    private string? repositoryRoot;

    public IBrowser? Browser { get; private set; }
    public static string TestHost => Environment.GetEnvironmentVariable("KAFE_E2E_HOST") ?? "localhost";
    public string BaseUrl => $"https://{TestHost}:3001";
    public string ApiBaseUrl => $"https://{TestHost}:44369";
    public string GroupPath => $"/auth/{OrganizationId}/groups/{GroupId}";
    public string ArtifactsDirectory => Environment.GetEnvironmentVariable("KAFE_E2E_ARTIFACTS")
        ?? Path.Combine(RepositoryRoot, "EndToEnd", "artifacts");

    private string RepositoryRoot => repositoryRoot ??=
        Environment.GetEnvironmentVariable("KAFE_REPOSITORY_ROOT") ?? FindRepositoryRoot();

    public async Task InitializeAsync()
    {
        try
        {
            EnsureChromiumIsAvailable();

            Directory.CreateDirectory(ArtifactsDirectory);
            var viteConfig = Path.Combine(ArtifactsDirectory, "vite.config.e2e.ts");
            await File.WriteAllTextAsync(viteConfig,
                """
                import { mergeConfig } from 'vite';
                import baseConfig from '/app/vite.config.ts';

                export default mergeConfig(baseConfig, {
                    server: {
                        proxy: {
                            '/api': {
                                target: 'https://api:44369',
                                secure: false,
                            },
                        },
                    },
                });
                """);

            await BuildImageAsync("api", "Api.Dockerfile");
            await BuildImageAsync("web", "Web.Dockerfile", "builder");
            await BuildImageAsync("blender", "Blender.Dockerfile");

            var composeOverride = Path.Combine(ArtifactsDirectory, "docker-compose.override.yml");
            await File.WriteAllTextAsync(composeOverride,
                $"""
                services:
                  api:
                    image: kafe-e2e-api:local
                    depends_on:
                      db:
                        condition: service_healthy
                      blender:
                        condition: service_started
                    environment:
                      FrontEndUrl: https://{TestHost}:3001
                      VideoConversion__PollWaitTime: 00:00:01
                  web:
                    image: kafe-e2e-web:local
                    command: ["pnpm", "run", "start", "--", "--config", "/tmp/vite.config.e2e.ts"]
                    ports:
                      - "3001:3000"
                    volumes:
                      - '{viteConfig}:/tmp/vite.config.e2e.ts:ro'
                  blender:
                    image: kafe-e2e-blender:local
                  db:
                    healthcheck:
                      test: ["CMD-SHELL", "pg_isready -U postgres -d postgres"]
                      interval: 2s
                      timeout: 3s
                      retries: 30
                """);

            // docker-compose.base.yml uses this external network for its Traefik alias.
            var backbone = new NetworkBuilder().WithName("backbone").WithReuse(true).Build();
            await backbone.CreateAsync();

            stack = new ComposeBuilder("docker:29-cli")
                .WithEnvironment("COMPOSE_PARALLEL_LIMIT", "1")
                .WithComposeFile(
                    Path.Combine(RepositoryRoot, "docker-compose.base.yml"),
                    Path.Combine(RepositoryRoot, "docker-compose.local.yml"),
                    composeOverride)
                .WithCopyFilesInContainer(
                    ".dockerignore",
                    "docker-compose.base.yml",
                    "docker-compose.local.yml",
                    "Api", "Api.Dockerfile",
                    "Blender.Dockerfile",
                    "Common", "Data", "Media", "Pigeons",
                    "Directory.Build.props", "Directory.Build.targets",
                    "Web", "Web.Dockerfile",
                    "EndToEnd/artifacts")
                .WithProjectNamePrefix("kafe-e2e-")
                .WithComposeUpOption("--no-build")
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

        foreach (var image in images)
        {
            await image.DeleteAsync();
        }
    }

    private async Task BuildImageAsync(string name, string dockerfile, string? target = null)
    {
        var builder = new ImageFromDockerfileBuilder()
            .WithName($"kafe-e2e-{name}:local")
            .WithDockerfile(dockerfile)
            .WithDockerfileDirectory(RepositoryRoot)
            .WithContextDirectory(RepositoryRoot);
        if (target is not null)
        {
            builder = builder.WithTarget(target);
        }

        var image = builder.Build();
        await image.CreateAsync();
        images.Add(image);
        await PruneDanglingImagesAsync();
    }

    private static async Task PruneDanglingImagesAsync()
    {
        var dockerEndpoint = TestcontainersSettings.OS.DockerEndpointAuthConfig.Endpoint;
        var pruneBuilder = new ContainerBuilder("docker:29-cli")
            .WithEntrypoint("docker", "image", "prune", "--force")
            .WithEnvironment("DOCKER_HOST", dockerEndpoint.AbsoluteUri);
        if (dockerEndpoint.Scheme == "unix")
        {
            pruneBuilder = pruneBuilder.WithBindMount(dockerEndpoint.AbsolutePath, dockerEndpoint.AbsolutePath);
        }

        await using var prune = pruneBuilder.Build();
        await prune.StartAsync();
        var exitCode = await prune.GetExitCodeAsync();
        var logs = await prune.GetLogsAsync(timestampsEnabled: false);
        Assert.True(exitCode == 0, $"Could not prune build intermediates: {logs.Stdout}{logs.Stderr}");
    }

    private static void EnsureChromiumIsAvailable()
    {
        var exitCode = Program.Main(["install", "--with-deps", "--no-shell", "chromium"]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited while installing Chromium with code {exitCode}.");
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
