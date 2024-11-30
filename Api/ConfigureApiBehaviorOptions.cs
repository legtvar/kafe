using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions o)
    {
        o.InvalidModelStateResponseFactory = ctx => KafeProblemDetails.Create(
            httpContext: ctx.HttpContext,
            modelState: ctx.ModelState,
            skipFrames: 2 // skip the Create method and this lambda
        ).ToActionResult();
    }
}
