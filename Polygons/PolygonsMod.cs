namespace Kafe.Polygons;

[Mod("polygons")]
public class PolygonsMod : IMod
{
    public KafeType BlendShardType { get; private set; }

    public void Configure(ModContext context)
    {
        BlendShardType = context.AddShard<BlendInfo>(new()
        {
            Name = "blend",
        });
    }
}
