using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Kafe.Api;

public static class ControllerBaseExtensions
{
    extension(ControllerBase controller)
    {
        public ActionResult KafeErrResult<T>(Err<T> err)
        {
            if (err.HasError)
            {
                var factory = controller.HttpContext.RequestServices.GetRequiredService<KafeProblemDetailsFactory>();
                return factory.CreateProblemDetails(err.Diagnostic).ToActionResult();
            }

            return new OkObjectResult(err.Value);
        }

        public ActionResult KafeErrorResult(params IEnumerable<Diagnostic> diagnostics)
        {
            var diagnostic = Diagnostic.Aggregate(diagnostics);
            var factory = controller.HttpContext.RequestServices.GetRequiredService<KafeProblemDetailsFactory>();
            return factory.CreateProblemDetails(diagnostic).ToActionResult();
        }
    }
}
