using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions o)
    {
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var pd = KafeProblemDetails.Create(
                httpContext: ctx.HttpContext,
                modelState: ctx.ModelState,
                skipFrames: 2 // skip the Create method and this lambda
            );
            var objectResult = new ObjectResult(pd)
            {
                StatusCode = pd.Status
            };
            objectResult.ContentTypes.Add("application/problem+json");
            objectResult.ContentTypes.Add("application/problem+xml");

            return objectResult;
        };
    }
}
