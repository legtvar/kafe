namespace Kafe.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization();
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
