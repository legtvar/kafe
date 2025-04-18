using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Polygons;

public class PolygonsMod : IMod
{
    public static string Name {get;} = "polygons";
    
    public KafeType BlendShardType { get; private set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<BlendShardAnalyzer>();
    }

    public void Configure(ModContext context)
    {
        BlendShardType = context.AddShard<BlendInfo>(new()
        {
            Name = "blend",
            AnalyzerTypes = [typeof(BlendShardAnalyzer)]
        });
    }
}
