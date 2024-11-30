using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Kafe;

public readonly partial record struct Error
{
    public const string GenericErrorId = "GenericError";

    public Error(
        string id,
        string message,
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
            message,
            ImmutableDictionary<string, object>.Empty,
            null,
            skipFrames: 2) // to skip this frame and `this()`
    {
    }

    public Error(string id, string message)
        : this(
            id,
            message,
            ImmutableDictionary<string, object>.Empty,
            null,
            skipFrames: 2) // to skip this frame and `this()`
    {
    }

    public Error(Exception inner, string? stackTrace = null, int skipFrames = 1)
        : this(
            inner.GetType().FullName ?? inner.GetType().Name,
            inner.Message,
            ImmutableDictionary<string, object>.Empty,
            stackTrace,
            skipFrames: skipFrames + 1)
    {
        InnerException = inner;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Exception? InnerException { get; init; }

    public string Id { get; init; }

    public string Message { get; init; }

    public ImmutableDictionary<string, object> Arguments { get; init; }
        = ImmutableDictionary<string, object>.Empty;

    // NB: We do actually set it to null in ErrorJsonConverter.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string StackTrace { get; init; }

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

    public Error WithArgument(string key, object value)
    {
        return this with
        {
            Arguments = Arguments.Add(key, value)
        };
    }
}
