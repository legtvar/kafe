using System.Security.Claims;
using idunno.Authentication.Basic;
using Kafe.Data;
using Kafe.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

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
            o.EnableAnnotations();
        });

        Db.AddDb(services, Configuration, Environment);
        services.AddControllers(o =>
        {
            o.Conventions.Add(new RoutePrefixConvention(new RouteAttribute("/api/v{version:apiVersion}")));
        });
        services.AddApiVersioning(o => o.ReportApiVersions = true);
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment environment)
    {
        app.UseHttpsRedirection();

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
