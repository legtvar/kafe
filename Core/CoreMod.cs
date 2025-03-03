using System;

namespace Kafe.Core;

[Mod(Name)]
public sealed class CoreMod : IMod
{
    public const string Name = "core";
    
    public void Configure(ModContext context)
    {
        context.AddType<LocalizedString>();
        
    }
}
