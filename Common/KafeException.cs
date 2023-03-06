using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

/// <summary>
/// Use in dire cases that should "never" happen.
/// </summary>
[Serializable]
public class KafeException : Exception
{
    public KafeException() { }
    public KafeException(string message) : base(message) { }
    public KafeException(string message, Exception inner) : base(message, inner) { }
    protected KafeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
