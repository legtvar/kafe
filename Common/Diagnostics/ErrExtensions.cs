using System;

namespace Kafe;

public static class ErrExtensions
{
    extension<T>(Err<T> err)
    {
        public Err<T> ForParameter(string parameterName, int skipFrames = 1)
        {
            if (!err.Diagnostic.IsValid)
            {
                throw new InvalidOperationException("Cannot wrap an invalid diagnostic into a parameter-bound diagnostic.");
            }

            return err.Diagnostic.ForParameter(parameterName, skipFrames + 1);
        }
    }
}
