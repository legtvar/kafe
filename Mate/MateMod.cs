using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Mate;

public class MateMod : IMod
{
    public static string Moniker => "mate";

    public void Configure(ModContext context)
    {
        ConfigureServices(context.Services);

        context.AddShardPayload<BlendInfo>(new ShardPayloadRegistrationOptions
        {
            AnalyzerTypes = [typeof(BlendShardAnalyzer)]
        });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<BlendShardAnalyzer>();
        services.AddSingleton<PigeonsTestQueue>();
        services.AddScoped<PigeonsService>();
    }
}
