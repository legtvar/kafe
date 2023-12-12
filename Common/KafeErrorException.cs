namespace Kafe.Common;

public class KafeErrorException : Exception
{
    public KafeErrorException(Error error) : base(error.Message, error.InnerException)
    {
        InnerError = error;
    }
    
    public Error InnerError { get; }

    public override string? StackTrace => InnerError.StackTrace;
}
