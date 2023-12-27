using System;
using System.Collections.Immutable;

namespace Kafe.Common;

public class KafeErrorException : Exception
{
    public KafeErrorException(Error error) : base(error.Message, error.InnerException)
    {
        InnerErrors = ImmutableArray.Create(error);
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
