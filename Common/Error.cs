using System.Diagnostics;

namespace Kafe.Common;

public readonly record struct Error
{
    public Error(string message, string? stackTrace = null, int skipFrames = 1)
    {
        Message = message;
        StackTrace = stackTrace ?? new StackTrace(skipFrames: skipFrames, fNeedFileInfo: true).ToString();
    }

    public Error(FormattableString formattedMessage, string? stackTrace = null, int skipFrames = 1) : this(
        formattedMessage.ToString(),
        stackTrace,
        skipFrames: skipFrames + 1)
    {
        FormattedMessage = formattedMessage;
    }

    public Error(Exception inner, string? stackTrace = null, int skipFrames = 1) : this(
        inner.Message,
        stackTrace,
        skipFrames: skipFrames + 1)
    {
        InnerException = inner;
    }

    public Exception? InnerException { get; }

    public FormattableString? FormattedMessage { get; }

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
