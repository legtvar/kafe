using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Mate;

public class MateMod : IMod
{
    public static string Moniker => "mate";

    public KafeType BlendShardType { get; private set; }

    public void Configure(ModContext context)
    {
        ConfigureServices(context.Services);

        BlendShardType = context.AddShard<BlendInfo>(new()
        {
            Moniker = "blend",
            AnalyzerTypes = [typeof(BlendShardAnalyzer)]
        });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<BlendShardAnalyzer>();
    }
}
