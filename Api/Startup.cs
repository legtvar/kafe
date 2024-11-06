using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Kafe.Api.Swagger;
using Kafe.Api.Services;
using Asp.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System;
using Microsoft.OpenApi.Any;
using System.IO;
using System.Collections.Immutable;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Kafe.Api.Options;
using Kafe.Data.Options;
using Kafe.Api.Daemons;
using Kafe.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Kafe.Media.Services;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using System.Text.Json;
using Kafe.Data.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Npgsql;

namespace Kafe.Api;

public class Startup
{
    public IConfiguration Configuration { get; }
    public ApiOptions ApiOptions { get; }
    public IHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        ApiOptions = Configuration.Get<ApiOptions>()!;
        Environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        IdentityModelEventSource.ShowPII = true;

        var otlpEndpoint = Configuration.GetValue<string>("Otlp:Endpoint") ?? "http://localhost:4317";
        var otlpName = Configuration.GetValue<string>("Otlp:Name") ?? "Kafe";
        Log.Logger.Information("OTLP: Endpoint={OtlpEndpoint}, Name={Name}", otlpEndpoint, otlpName);

        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(Configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: Program.LogTemplate
            )
            .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = $"{otlpEndpoint}/v1/logs";
                    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = otlpName
                    };
                }
            )
        );
        services.AddSingleton<SmtpLogger>();

        services.AddHttpContextAccessor();

        services.AddAuthentication(o =>
        {
            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
        {
            o.Events.OnRedirectToAccessDenied = c =>
            {
                c.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
            o.Events.OnRedirectToLogin = c =>
            {
                c.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            o.AccessDeniedPath = "/error?title=403&detail='Access Denied'";
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, o =>
        {
            o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            var oidcConfig = Configuration.GetRequiredSection("Oidc").Get<OidcOptions>()
                ?? throw new ArgumentException("OIDC is not configured well.");

            o.Authority = oidcConfig.Authority;
            o.ClientId = oidcConfig.ClientId;
            o.ClientSecret = oidcConfig.ClientSecret;
            o.ResponseType = OpenIdConnectResponseType.Code;
            o.Scope.Add("openid");
            o.Scope.Add("profile");
            o.Scope.Add("email");
            o.CallbackPath = new PathString("/signin-oidc");
            // o.GetClaimsFromUserInfoEndpoint = true;
            // o.SaveTokens = true;

            o.Events.OnRemoteFailure += c =>
            {
                var clientHost = Uri.TryCreate(c.Properties?.RedirectUri, UriKind.Absolute, out var redirect)
                    ? redirect.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
                    : null;
                c.Response.Redirect($"{clientHost}/error?title=External Login Failed&detail={c.Failure?.Message}");
                c.HandleResponse();
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization(o =>
        {
            o.AddPolicy(EndpointPolicy.Read, b => b.AddRequirements(new PermissionRequirement(Permission.Read)));
            o.AddPolicy(EndpointPolicy.Write, b => b.AddRequirements(new PermissionRequirement(Permission.Write)));
            o.AddPolicy(EndpointPolicy.Append, b => b.AddRequirements(new PermissionRequirement(Permission.Append)));
            o.AddPolicy(EndpointPolicy.Inspect, b => b.AddRequirements(new PermissionRequirement(Permission.Inspect)));
            o.AddPolicy(EndpointPolicy.ReadInspect, b => b.AddRequirements(new PermissionRequirement(Permission.Read | Permission.Inspect)));
            o.AddPolicy(EndpointPolicy.Review, b => b.AddRequirements(new PermissionRequirement(Permission.Review)));
        });

        ConfigureDataProtection(services);

        services.AddEndpointsApiExplorer();

        services.AddApiVersioning(o =>
        {
            o.ReportApiVersions = true;
            o.DefaultApiVersion = new ApiVersion(1);
        });
        services.AddSwaggerGen(ConfigureSwaggerGen);

        services.AddControllers(o =>
        {
            o.Conventions.Add(new RoutePrefixConvention(new RouteAttribute("/api/v{version:apiVersion}")));
            o.Filters.Add(typeof(SemanticExceptionFilter));
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            o.JsonSerializerOptions.Converters.Add(new LocalizedStringJsonConverter());
        });

        services.AddCors(o =>
        {
            o.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader();
                p.AllowAnyMethod();
                p.AllowCredentials();
                p.SetIsOriginAllowedToAllowWildcardSubdomains();
                p.WithOrigins(ApiOptions.AllowedOrigins.ToArray());
            });
        });

        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.All;

            // Accept proxies at all local IPv4 addresses
            o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("192.168.0.0"), 16));
            o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
            o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
        });

        services.AddProblemDetails();

        // services.AddHttpLogging(o =>
        // {
        // });

        RegisterKafe(services);

        var otel = services.AddOpenTelemetry();
        otel.ConfigureResource(r => r
            .AddTelemetrySdk()
            .AddService(serviceName: otlpName));
        otel.WithMetrics(m => m
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
            }));
        otel.WithTracing(t => t
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(otlpName)
            .AddNpgsql()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
            }));
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseForwardedHeaders();

        app.UseHttpsRedirection();

        var apiOptions = app.ApplicationServices.GetRequiredService<IOptions<ApiOptions>>();
        app.UseRewriter(new RewriteOptions()
            .AddRewrite($"^{apiOptions.Value.AccountConfirmPath.Trim('/')}/(.*)$", "api/v1/tmp-account/$1", true));

        app.UseSerilogRequestLogging();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.Use(async (ctx, next) =>
        {
            var emailClaim = ctx.User.FindFirst(ClaimTypes.Email);
            if (emailClaim is null || string.IsNullOrEmpty(emailClaim.Value))
            {
                await next();
                return;
            }

            var oidcConfig = Configuration.GetRequiredSection("Oidc").Get<OidcOptions>()
                ?? throw new ArgumentException("OIDC is not configured well.");

            if (emailClaim.Issuer.StartsWith(oidcConfig.Authority))
            {
                var result = await ctx.RequestServices.GetRequiredService<AccountService>()
                    .AssociateExternalAccount(ctx.User);
                if (result.HasErrors)
                {
                    await ctx.ForbidAsync();
                    return;
                }

                await ctx.RequestServices.GetRequiredService<UserProvider>().SignIn(result.Value);
            }

            await ctx.RequestServices.GetRequiredService<UserProvider>().RefreshAccount();
            await next();
        });
        app.UseAuthorization();

        app.UseExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI();


        app.UseEndpoints(e =>
        {
            e.MapControllers();
        });
    }

    private void RegisterKafe(IServiceCollection services)
    {
        services.AddKafeMedia();
        services.AddKafeData();

        services.AddScoped<UserProvider>();

        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        services.AddOptions<ApiOptions>()
            .Bind(Configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<EmailOptions>()
            .BindConfiguration("Email")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<VideoConversionDaemon>();

        var emailServiceType = Configuration.GetSection("Email").Get<EmailOptions>()?.ServiceType
            ?? EmailOptions.EmailServiceType.Default;
        switch (emailServiceType)
        {
            case EmailOptions.EmailServiceType.Debug:
                services.AddSingleton<IEmailService, DebugEmailService>();
                break;
            case EmailOptions.EmailServiceType.Relayed:
                services.AddSingleton<IEmailService, RelayedEmailService>();
                break;
            default:
                services.AddSingleton<IEmailService, DefaultEmailService>();
                break;
        }

    }

    private void ConfigureDataProtection(IServiceCollection services)
    {
        var secretsDirPath = Configuration.GetRequiredSection("Storage").Get<StorageOptions>()?.SecretsDirectory;
        if (secretsDirPath is null)
        {
            throw new ArgumentException($"The {nameof(StorageOptions.SecretsDirectory)} is not set.");
        }

        var exeDir = new DirectoryInfo(Path.GetDirectoryName(typeof(Startup).Assembly.Location)!);
        if (!exeDir.Exists)
        {
            throw new InvalidOperationException("Could not find the executable's directory.");
        }

        var secretsFullPath = Path.Combine(exeDir.FullName, secretsDirPath);

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(secretsFullPath));
    }

    private void ConfigureSwaggerGen(SwaggerGenOptions o)
    {
        o.SwaggerDoc("v1", new OpenApiInfo { Title = "KAFE API", Version = "v1" });
        o.SupportNonNullableReferenceTypes();
        o.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        o.MapType<Hrib>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "hrib",
            Example = new OpenApiString("AAAAbadf00d")
        });
        o.MapType<TimeSpan>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "time-span",
            Example = new OpenApiString("00:00:00")
        });
        o.MapType<LocalizedString>(() => new OpenApiSchema
        {
            Title = "LocalizedString",
            Type = "object",
            Nullable = true,
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["iv"] = new OpenApiSchema() { Type = "string" },
                ["cs"] = new OpenApiSchema() { Type = "string", Nullable = true },
                ["en"] = new OpenApiSchema() { Type = "string", Nullable = true }
            }
        });
        // o.MapType<Permission>(() => new OpenApiSchema
        // {
        //     Title = nameof(Permission),
        //     Ref
        //     Type = "string",
        //     Enum = Enum.GetValues<Permission>()
        //         .Where(v => v != Permission.None && v != Permission.All)
        //         .Select(v =>
        //         {
        //             var name = v.ToString();
        //             name = char.ToLowerInvariant(name[0]) + name[1..];
        //             return new OpenApiString(name) as IOpenApiAny;
        //         })
        //         .ToList()
        // });
        o.EnableAnnotations(
            enableAnnotationsForInheritance: true,
            enableAnnotationsForPolymorphism: true);
        o.OperationFilter<RemoveVersionParameter>();
        o.DocumentFilter<ReplaceVersionWithDocVersion>();
        o.DocInclusionPredicate((version, desc) =>
        {
            if (!desc.TryGetMethodInfo(out var method))
            {
                return false;
            }

            var versions = method.DeclaringType!.GetCustomAttributes(true)
                .OfType<ApiVersionAttribute>()
                .SelectMany(attr => attr.Versions);

            var maps = method.GetCustomAttributes(true)
                .OfType<MapToApiVersionAttribute>()
                .SelectMany(attr => attr.Versions)
                .ToArray();

            return versions.Any(v => $"v{v}" == version) && (maps.Length == 0 || maps.Any(v => $"v{v}" == version));
        });
        o.UseOneOfForPolymorphism();
        var xmlFiles = new DirectoryInfo(AppContext.BaseDirectory).GetFiles("*.xml");
        foreach (var xmlFile in xmlFiles)
        {
            o.IncludeXmlComments(xmlFile.FullName);
        }
    }
}
