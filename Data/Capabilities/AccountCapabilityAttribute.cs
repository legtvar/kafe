using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Capabilities;

public sealed class AccountCapabilityAttribute : KafeTypeAttribute
{
    public AccountCapabilityAttribute() : base(typeof(AccountCapability))
    {
    }
}
