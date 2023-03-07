using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class KafeTypeAttribute : Attribute
{
    public KafeTypeAttribute(Type type)
    {
        Type = type;
    }
    public Type Type { get; }
}
