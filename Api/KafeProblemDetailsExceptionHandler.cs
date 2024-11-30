using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kafe.Api;

public class KafeProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService problemDetailsService;
    private readonly ILogger<KafeProblemDetailsExceptionHandler> logger;

    public KafeProblemDetailsExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<KafeProblemDetailsExceptionHandler> logger)
    {
        this.problemDetailsService = problemDetailsService;
        this.logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var pd = KafeProblemDetails.Create(
            httpContext: httpContext,
            statusCode: 500
        );
        pd.Errors = [exception];
        
        var endpoint = httpContext.GetEndpoint();
        
        await problemDetailsService.WriteAsync(new()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = pd,
            AdditionalMetadata = endpoint?.Metadata
        });
        
        logger.LogError(
            exception,
            "An exception occured while processing '{EndpointDisplayName}'.",
            endpoint?.DisplayName ?? "Unknown endpoint");
        
        return true;
    }
}
