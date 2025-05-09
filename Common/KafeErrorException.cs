using System;
using System.Collections.Immutable;
using System.Linq;

namespace Kafe;

public class KafeErrorException : Exception
{
    public KafeErrorException(Error error) : base(error.Message.ToString(), error.InnerException)
    {
        InnerErrors = [error];
        StackTrace = error.StackTrace;
    }

    public KafeErrorException(ImmutableArray<Error> errors)
        : base($"One or more KAFE errors occurred:{string.Join("\n- ", errors.Select(e => e.Message))}")
    {
        InnerErrors = errors;
        StackTrace = errors.FirstOrDefault().StackTrace;
    }

    public ImmutableArray<Error> InnerErrors { get; }

    public override string? StackTrace { get; }
}
