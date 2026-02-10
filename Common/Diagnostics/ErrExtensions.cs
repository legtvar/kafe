using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public Err<T> Combine(params IEnumerable<Diagnostic> diagnostics)
        {
            var validDiagnostics = diagnostics.Where(d => d.IsValid).ToImmutableArray();
            if (!validDiagnostics.Any())
            {
                return err;
            }

            return err.Diagnostic switch
            {
                { IsValid: false } or { Payload: GenericErrorDiagnostic }
                    => new Err<T>(err.Value, Diagnostic.Aggregate(validDiagnostics)),
                { Payload: AggregateDiagnostic agg }
                    => new Err<T>(err.Value, Diagnostic.Aggregate(agg.Inner.AddRange(validDiagnostics))),
                _ => new Err<T>(err.Value, Diagnostic.Aggregate([err.Diagnostic, ..validDiagnostics]))
            };
        }

        public Err<TInner> Select<TInner>(Func<T,TInner> selector)
        {
            return err.HasError ? err.Diagnostic : Err.Succeed(selector(err.Value));
        }
    }
}
