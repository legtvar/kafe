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
using Kafe.Media;
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

namespace Kafe.Api;

public class Startup
{
    public IConfiguration Configuration { get; }
    public IHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();

        services.AddAuthorization(o =>
        {
            o.AddPolicy(EndpointPolicy.AdministratorOnly, b => b.AddRequirements(new AdministratorRequirement()));
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
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.Converters.Add(new LocalizedStringJsonConverter());
        });

        RegisterKafe(services);
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment environment)
    {
        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseEndpoints(e =>
        {
            e.MapControllers();
        });
    }

    private void RegisterKafe(IServiceCollection services)
    {
        Db.AddDb(services, Configuration, Environment);

        services.AddSingleton<IMediaService, FFmpegCoreService>();

        services.AddScoped<ICurrentAccountProvider, DefaultCurrentAccountProvider>();
        services.AddScoped<IProjectService, DefaultProjectService>();
        services.AddScoped<IAuthorService, DefaultAuthorService>();
        services.AddScoped<IArtifactService, DefaultArtifactService>();
        services.AddScoped<IShardService, DefaultShardService>();
        services.AddScoped<IAccountService, DefaultAccountService>();

        services.AddScoped<IAuthorizationHandler, AdministratorHandler>();

        services.Configure<ApiOptions>(Configuration);
        services.Configure<EmailOptions>(Configuration.GetSection("Email"));
        services.Configure<SeedOptions>(Configuration.GetSection("Seed"));

        services.AddHostedService<SeedDaemon>();

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
