using Microsoft.Extensions.DependencyInjection;

namespace Kafe;

public interface IMod
{
    void ConfigureServices(IServiceCollection serviceCollection)
    {}

    void Configure(ModContext context);
}
