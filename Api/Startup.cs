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
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo { Title = "KAFE API", Version = "v1" });
            o.SupportNonNullableReferenceTypes();
            o.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
            o.EnableAnnotations();
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
        services.AddApiVersioning(o => o.ReportApiVersions = true);

        // KAFE services
        services.AddSingleton<IMediaService, XabeFFmpegService>();
        services.AddScoped<IArtifactService, DefaultArtifactService>();
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
