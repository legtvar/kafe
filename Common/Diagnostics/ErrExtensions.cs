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
                throw new InvalidOperationException(
                    "Cannot wrap an invalid diagnostic into a parameter-bound diagnostic."
                );
            }

            return err.Diagnostic.ForParameter(parameterName, skipFrames + 1);
        }

        public Err<T> Combine(Diagnostic diagnostic)
        {
            if (!diagnostic.IsValid)
            {
                return err;
            }

            return err.Diagnostic switch
            {
                { IsValid: false } or { Payload: GenericErrorDiagnostic }
                    => new Err<T>(err.Value, diagnostic),
                { Payload: AggregateDiagnostic agg }
                    => new Err<T>(err.Value, Diagnostic.Aggregate(agg.Inner.Add(diagnostic))),
                _ => new Err<T>(err.Value, Diagnostic.Aggregate([err.Diagnostic, diagnostic]))
            };
        }
    }
}
