using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Kafe.Common;

public readonly partial record struct Error
{
    public const string GenericErrorId = "GenericError";

    public Error(
        string id,
        LocalizedString message,
        ImmutableDictionary<string, object> arguments,
        string? stackTrace = null,
        int skipFrames = 1)
    {
        Id = id;
        Message = message;
        Arguments = arguments;
        StackTrace = stackTrace ?? new StackTrace(skipFrames: skipFrames, fNeedFileInfo: true).ToString();
    }

    public Error(string message)
        : this(
            GenericErrorId,
            LocalizedString.CreateInvariant(message),
            ImmutableDictionary<string, object>.Empty,
            null,
            skipFrames: 2) // to skip this frame and `this()`
    {
    }

    public Error(string id, string message)
        : this(
            id,
            LocalizedString.CreateInvariant(message),
            ImmutableDictionary<string, object>.Empty,
            null,
            skipFrames: 2) // to skip this frame and `this()`
    {
    }

    public Error(Exception inner, string? stackTrace = null, int skipFrames = 1)
        : this(
            inner.GetType().FullName ?? inner.GetType().Name,
            LocalizedString.CreateInvariant(inner.Message),
            ImmutableDictionary<string, object>.Empty,
            stackTrace,
            skipFrames: skipFrames + 1)
    {
        InnerException = inner;
    }

    public Exception? InnerException { get; }

    public string Id { get; }

    public LocalizedString Message { get; }

    public ImmutableDictionary<string, object> Arguments { get; }
        = ImmutableDictionary<string, object>.Empty;

    public string StackTrace { get; }

    // NB: This operator is "explicit" so that it is not so easy to create multiply nested error-exception-errors
    //     by accident.
    public static explicit operator Exception(Error error)
    {
        return new KafeErrorException(error);
    }

    public static implicit operator Error(Exception exception)
    {
        return new Error(exception, skipFrames: 2);
    }
}
