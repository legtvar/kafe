using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Kafe.Api;

public class KafeProblemDetailsService : IProblemDetailsService
{
    private readonly IProblemDetailsWriter[] writers;

    public KafeProblemDetailsService(IEnumerable<IProblemDetailsWriter> writers)
    {
        this.writers = writers.ToArray();
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context.ProblemDetails);
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        KafeProblemDetails pd;
        if (context.ProblemDetails is KafeProblemDetails kpd)
        {
            pd = kpd;
        }
        else
        {
            pd = KafeProblemDetails.Create(
                httpContext: context.HttpContext,
                statusCode: context.ProblemDetails.Status,
                title: context.ProblemDetails.Title,
                type: context.ProblemDetails.Type,
                detail: context.ProblemDetails.Detail,
                instance: context.ProblemDetails.Instance,
                skipFrames: 2
            );
        }

        context.ProblemDetails = pd;

        for (int i = 0; i < writers.Length; ++i)
        {
            var current = writers[i];
            if (current.CanWrite(context))
            {
                await current.WriteAsync(context);
            }
        }
    }
}
