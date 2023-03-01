using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using idunno.Authentication.Basic;
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
        services.AddAuthentication("basic")
            .AddBasic("basic", o =>
            {
                o.Realm = "Basic Authentication";
                o.AllowInsecureProtocol = true;
                o.Events = new BasicAuthenticationEvents
                {
                    OnValidateCredentials = context =>
                    {
                        if (context.Username == Configuration["Authentication:Basic:Username"]
                            && context.Password == Configuration["Authentication:Basic:Password"])
                        {
                            var claims = new[]
                            {
                                new Claim(
                                    ClaimTypes.NameIdentifier,
                                    context.Username,
                                    ClaimValueTypes.String,
                                    context.Options.ClaimsIssuer),
                                new Claim(
                                    ClaimTypes.Name,
                                    context.Username,
                                    ClaimValueTypes.String,
                                    context.Options.ClaimsIssuer)
                            };

                            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                            context.Success();
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(o =>
        {
            o.ReportApiVersions = true;
            o.DefaultApiVersion = new ApiVersion(1);
        });
        services.AddSwaggerGen(o =>
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
        });

        Db.AddDb(services, Configuration, Environment);
        services.AddControllers(o =>
        {
            o.Conventions.Add(new RoutePrefixConvention(new RouteAttribute("/api/v{version:apiVersion}")));
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.Converters.Add(new LocalizedStringJsonConverter());
        });

        // KAFE services
        services.AddSingleton<IMediaService, XabeFFmpegService>();
        services.AddScoped<IProjectService, DefaultProjectService>();
        services.AddScoped<IAuthorService, DefaultAuthorService>();
        services.AddScoped<IArtifactService, DefaultArtifactService>();
        services.AddScoped<IShardService, DefaultShardService>();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment environment)
    {
        app.UseHttpsRedirection();

        app.UseRewriter(new RewriteOptions().AddRewrite("login", "login.html", true));
        // app.Use(async (context, next) => {
        //     if (context.Request.Path.Value == "/login") {

        //     }
        //     await next(context);
        // });

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
}
