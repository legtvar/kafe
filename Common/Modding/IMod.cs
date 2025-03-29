using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public interface IMod
{
    void ConfigureServices(IServiceCollection services)
    {
    }

    void Configure(ModContext context);
}
