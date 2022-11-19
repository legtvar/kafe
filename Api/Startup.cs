using Kafe.Data;
using Marten;
using Marten.Events;
using Weasel.Core;

namespace Kafe.Api;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, IHostEnvironment environment)
    {
        services.AddAuthorization();
        Db.AddDb(services, Configuration, environment);
    }
    
    public void Configure(IApplicationBuilder app, IHostEnvironment environment)
    {
        // Configure the HTTP request pipeline.
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        // app.MapControllers();
    }
}
