using System.Diagnostics;

namespace Kafe.Common;

public readonly record struct Error
{
    public const string GenericErrorId = "GenericError";

    public Error(string id, string message, string? stackTrace = null, int skipFrames = 1)
    {
        Id = id;
        Message = message;
        StackTrace = stackTrace ?? new StackTrace(skipFrames: skipFrames, fNeedFileInfo: true).ToString();
    }

    public Error(string id, FormattableString formattedMessage, string? stackTrace = null, int skipFrames = 1)
        : this(
            id,
            formattedMessage.ToString(),
            stackTrace,
            skipFrames: skipFrames + 1)
    {
        FormattedMessage = formattedMessage;
    }

    public Error(string message)
        : this(
            GenericErrorId,
            message,
            null,
            skipFrames: 2) // to skip this frame and `this()`
    {
    }

    public Error(FormattableString formattedMessage)
        : this(
            GenericErrorId,
            formattedMessage,
            null,
            skipFrames: 2) // to skip this frame and `this()`
    {
    }

    public Error(Exception inner, string? stackTrace = null, int skipFrames = 1)
        : this(
            inner.GetType().FullName ?? inner.GetType().Name,
            inner.Message,
            stackTrace,
            skipFrames: skipFrames + 1)
    {
        InnerException = inner;
    }

    public Exception? InnerException { get; }

    public FormattableString? FormattedMessage { get; }

    public string Id { get; }

    public string Message { get; }

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

    public static implicit operator string(Error error)
    {
        return error.Message;
    }
}
