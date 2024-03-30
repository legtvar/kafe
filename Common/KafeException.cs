using System;

namespace Kafe;

/// <summary>
/// Use in dire cases that should "never" happen.
/// </summary>
public class KafeException : Exception
{
    public KafeException() { }
    public KafeException(string message) : base(message) { }
    public KafeException(string message, Exception inner) : base(message, inner) { }
}
