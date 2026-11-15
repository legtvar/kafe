using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Kafe.Tests.EndToEnd;

public sealed class EndToEndFixture : IAsyncLifetime
{
    public const string GroupId = "test--group";
    public const string OrganizationId = "test----org";

    private ComposeContainer? stack;
    private IContainer? api;
    private IPlaywright? playwright;
    private string? repositoryRoot;
    private readonly IMessageSink diagnosticSink;

    public IBrowser? Browser { get; private set; }
    public static string TestHost => Environment.GetEnvironmentVariable("KAFE_E2E_HOST") ?? "localhost";
    public string BaseUrl => $"https://{TestHost}:3001";
    public string ApiBaseUrl => $"https://{TestHost}:44369";
    public string GroupPath => $"/auth/{OrganizationId}/groups/{GroupId}";

    public string ArtifactsDirectory => Environment.GetEnvironmentVariable("KAFE_E2E_ARTIFACTS")
        ?? Path.Combine(RepositoryRoot, "Tests.EndToEnd", "artifacts");

    private string RepositoryRoot => repositoryRoot ??=
        Environment.GetEnvironmentVariable("KAFE_REPOSITORY_ROOT") ?? FindRepositoryRoot();

    public ILoggerFactory LoggerFactory { get; }
    public ILogger FixtureLogger { get; }

    public EndToEndFixture(IMessageSink diagnosticSink)
    {
        this.diagnosticSink = diagnosticSink;
        var serilog = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Sink(new XunitDiagnosticSink(diagnosticSink))
            .CreateLogger();
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddSerilog(serilog, dispose: true);
        FixtureLogger = LoggerFactory.CreateLogger<EndToEndFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            FixtureLogger.LogInformation("Initializing the E2E fixture.");

            Directory.CreateDirectory(ArtifactsDirectory);
            var usePrebuiltImages = Environment.GetEnvironmentVariable("KAFE_E2E_USE_PREBUILT_IMAGES") == "1";
            var dockerEndpoint = new Uri(
                Environment.GetEnvironmentVariable("DOCKER_HOST")
                ?? TestcontainersSettings.OS.DockerEndpointAuthConfig.Endpoint.AbsoluteUri);
            if (!dockerEndpoint.Scheme.Equals("unix", StringComparison.OrdinalIgnoreCase))
            {
                // TestContainers insists on having the unix socket even if it doesn't use it because it accesses
                // Docker over TCP. So we create a directory and point the socket to that.
                var socketOverride = Path.GetFullPath(
                    Path.Combine(ArtifactsDirectory, "docker-socket"));
                Directory.CreateDirectory(socketOverride);
                TestcontainersSettings.DockerSocketOverride = socketOverride;
            }

            var viteConfig = Path.Combine(ArtifactsDirectory, "vite.config.e2e.ts");
            await File.WriteAllTextAsync(
                viteConfig,
                """
                import { mergeConfig } from 'vite';
                import baseConfig from '/app/vite.config.ts';

                export default mergeConfig(baseConfig, {
                    server: {
                        allowedHosts: ['{TestHost}'],
                        proxy: {
                            '/api': {
                                target: 'https://api:44369',
                                secure: false,
                            },
                        },
                    },
                });
                """
            );

            if (!usePrebuiltImages)
            {
                await BuildImageAsync("api", "Api.Dockerfile");
                await BuildImageAsync("web", "Web.Dockerfile", "builder");
                await BuildImageAsync("blender", "Blender.Dockerfile");
            }

            var composeOverride = Path.Combine(ArtifactsDirectory, "docker-compose.override.yml");
            await File.WriteAllTextAsync(
                composeOverride,
                $"""
                services:
                  api:
                    image: {GetServiceImage("api", usePrebuiltImages)}
                    depends_on:
                      db:
                        condition: service_healthy
                      blender:
                        condition: service_started
                    environment:
                      FrontEndUrl: https://{TestHost}:3001
                      VideoConversion__PollWaitTime: 00:00:01

                  {GetWebConfiguration()}

                  blender:
                    image: {GetServiceImage("blender", usePrebuiltImages)}

                  db:
                    healthcheck:
                      test: ["CMD-SHELL", "pg_isready -U postgres -d postgres"]
                      interval: 2s
                      timeout: 3s
                      retries: 30
                """
            );

            // docker-compose.base.yml uses this external network for its Traefik alias.
            var backbone = new NetworkBuilder()
                .WithName("backbone")
                .WithReuse(true)
                .WithLogger(FixtureLogger)
                .Build();
            await backbone.CreateAsync();

            var composeBuilder = new ComposeBuilder("docker:29-cli")
                .WithLogger(FixtureLogger);
            if (!dockerEndpoint.Scheme.Equals("unix", StringComparison.OrdinalIgnoreCase))
            {
                // The Docker hostname belongs to the CI job's network, not the new
                // container's network. Reach the remote DinD through its host gateway.
                composeBuilder = composeBuilder
                    .WithExtraHost("host.docker.internal", "host-gateway")
                    .WithEnvironment(
                        "DOCKER_HOST",
                        new UriBuilder(dockerEndpoint) { Host = "host.docker.internal" }.Uri.AbsoluteUri);
            }

            stack = composeBuilder
                .WithEnvironment("COMPOSE_PARALLEL_LIMIT", "1")
                .WithComposeFile(
                    Path.Combine(RepositoryRoot, "docker-compose.base.yml"),
                    Path.Combine(RepositoryRoot, "docker-compose.local.yml"),
                    composeOverride
                )
                .WithCopyFilesInContainer(
                    ".dockerignore",
                    "docker-compose.base.yml",
                    "docker-compose.local.yml",
                    "Api",
                    "Api.Dockerfile",
                    "Blender.Dockerfile",
                    "Common",
                    "Data",
                    "Media",
                    "Pigeons",
                    "Directory.Build.props",
                    "Directory.Build.targets",
                    "Web",
                    "Web.Dockerfile",
                    "Tests.EndToEnd/artifacts"
                )
                .WithProjectNamePrefix("kafe-e2e-")
                .WithComposeUpOption("--no-build")
                .WithPull(!usePrebuiltImages)
                .WithService("db", "api", "web")
                .WithExposedService("api", 44369)
                .WithExposedService("web", 3000)
                .Build();
            await stack.StartAsync();

            api = stack.GetServiceContainer("api");
            await api.ExecAsync(
                [
                    "mkdir", "-p",
                    "/mnt/kafe_generated/videos",
                    "/mnt/kafe_generated/images",
                    "/mnt/kafe_generated/subtitles",
                    "/mnt/kafe_generated/blends",
                    "/mnt/kafe_generated/unknown"
                ]
            );

            playwright = await Playwright.CreateAsync();
            if (!File.Exists(playwright.Chromium.ExecutablePath))
            {
                var exitCode = Program.Main(["install", "--with-deps", "--no-shell", "chromium"]);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Playwright exited while installing Chromium with code {exitCode}.");
                }
            }

            Browser = await playwright.Chromium.LaunchAsync(
                new()
                {
                    Headless = true,
                    Channel = "chromium",
                    Args = ["--no-sandbox", "--autoplay-policy=no-user-gesture-required"]
                }
            );
        }
        catch (Exception exception)
        {
            FixtureLogger.LogError(exception, "E2E fixture initialization failed.");
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

        var context = await Browser.NewContextAsync(
            new()
            {
                BaseURL = BaseUrl,
                IgnoreHTTPSErrors = true,
                Locale = "en-US",
                ViewportSize = new() { Width = 1440, Height = 1000 }
            }
        );
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
            RegexOptions.Compiled
        );

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

        throw new TimeoutException(
            $"No confirmation email for '{email}' appeared in the API logs within {maximumWait}."
        );
    }

    public async ValueTask DisposeAsync()
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

        LoggerFactory.Dispose();
    }

    private string GetServiceImage(string service, bool usePrebuiltImages)
    {
        if (usePrebuiltImages)
        {
            var registry = Environment.GetEnvironmentVariable("CI_REGISTRY_IMAGE");
            if (string.IsNullOrWhiteSpace(registry))
            {
                throw new InvalidOperationException(
                    "KAFE_E2E_USE_PREBUILT_IMAGES=1 requires CI_REGISTRY_IMAGE.");
            }

            var tag = Environment.GetEnvironmentVariable("CI_COMMIT_REF_SLUG") ?? "local";
            return $"{registry}/{service}:{tag}";
        }

        return $"kafe-e2e-{service}:local";
    }

    private string GetWebConfiguration()
    {
        return $"""
                  web:
                    image: {GetServiceImage("web", Environment.GetEnvironmentVariable("KAFE_E2E_USE_PREBUILT_IMAGES") == "1")}
                    command: ["pnpm", "run", "start", "--", "--config", "/tmp/vite.config.e2e.ts"]
                    ports:
                      - "3001:3000"
                    volumes:
                      - '{Path.Combine(ArtifactsDirectory, "vite.config.e2e.ts")}:/tmp/vite.config.e2e.ts:ro'
                """;
    }

    private async Task BuildImageAsync(string name, string dockerfile, string? target = null)
    {
        // Keep the images and their layers for subsequent test runs.
        var builder = new ImageFromDockerfileBuilder()
            .WithCleanUp(false)
            .WithLogger(FixtureLogger)
            .WithBuildArgument("KAFE_VERSION", "0.0.0-e2e")
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
