using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Kafe.Api;

public class KafeProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var pd = KafeProblemDetails.Create(
            httpContext: httpContext,
            statusCode: 500
        );
        pd.Diagnostics = [Diagnostic.FromException(exception)];

        var feature = httpContext.Features.GetRequiredFeature<IExceptionHandlerFeature>();

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = pd,
                AdditionalMetadata = feature.Endpoint?.Metadata
            }
        );

        // No need to do logging, since the ExceptionHandlerMiddleware and RequestLoggingMiddleware already doe it.

        return true;
    }
}
