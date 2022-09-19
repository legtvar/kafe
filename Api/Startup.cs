using Marten;
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
        services.AddMarten(options =>
        {
            options.Connection(Configuration.GetConnectionString("Marten"));
            if (environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
        });
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
