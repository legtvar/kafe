using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

public sealed class HribAttribute : KafeTypeAttribute
{
    public HribAttribute() : base(typeof(Hrib))
    {
    }
}
