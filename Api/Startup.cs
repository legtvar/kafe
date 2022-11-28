using Kafe.Data;
using Kafe.Endpoints;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Weasel.Core;

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
