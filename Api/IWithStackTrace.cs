using System.Diagnostics;

namespace Kafe.Api;

public interface IWithStackTrace
{
    StackTrace? StackTrace { get; set; }
}
