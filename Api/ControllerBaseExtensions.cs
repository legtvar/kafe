using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

public static class ControllerBaseExtensions
{
    public static ActionResult KafeErrResult<T>(this ControllerBase controller, Err<T> err)
    {
        if (!err.HasErrors)
        {
            return new OkObjectResult(err.Value);
        }

        var pd = KafeProblemDetails.Create(
            httpContext: controller.HttpContext,
            modelState: controller.ModelState,
            skipFrames: 2
        );
        pd.Errors = pd.Errors.AddRange(err.Errors);

        return new ObjectResult(pd)
        {
            StatusCode = pd.Status
        };
    }

    public static ActionResult KafeErrorResult(this ControllerBase controller, params IEnumerable<Error> errors)
    {
        var pd = KafeProblemDetails.Create(
            httpContext: controller.HttpContext,
            modelState: controller.ModelState,
            skipFrames: 2
        );
        pd.Errors = pd.Errors.AddRange(errors);

        return new ObjectResult(pd)
        {
            StatusCode = pd.Status
        };
    }
}
