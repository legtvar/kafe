using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kafe.Api;

public class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions o)
    {
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var factory = ctx.HttpContext.RequestServices.GetRequiredService<KafeProblemDetailsFactory>();
            var diagnostic = factory.CreateDiagnostic(
                ctx.ModelState,
                skipFrames: 2 // skip the Create method and this lambda
            );
            return factory.CreateProblemDetails(diagnostic).ToActionResult();
        };
    }
}
