using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

public static class ControllerBaseExtensions
{
    extension(ControllerBase controller)
    {
        public ActionResult KafeErrResult<T>(Err<T> err)
        {
            if (err.HasError)
            {
                var pd = KafeProblemDetails.Create(
                    httpContext: controller.HttpContext,
                    modelState: controller.ModelState,
                    skipFrames: 2
                );
                pd.Errors = pd.Errors.AddRange(err.Diagnostic);

                return new ObjectResult(pd)
                {
                    StatusCode = pd.Status
                };
            }

            return new OkObjectResult(err.Value);
        }

        public ActionResult KafeErrorResult(
            params IEnumerable<Diagnostic> diagnostics
        )
        {
            var pd = KafeProblemDetails.Create(
                httpContext: controller.HttpContext,
                modelState: controller.ModelState,
                skipFrames: 2
            );
            pd.Errors = pd.Errors.AddRange(diagnostics);

            return new ObjectResult(pd)
            {
                StatusCode = pd.Status
            };
        }
    }
}
