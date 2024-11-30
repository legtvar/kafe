using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kafe.Api;

public class KafeProblemDetailsClientErrorFactory : IClientErrorFactory
{
    public IActionResult? GetClientError(ActionContext actionContext, IClientErrorActionResult clientError)
    {
        return KafeProblemDetails.Create(
            httpContext: actionContext.HttpContext,
            modelState: actionContext.ModelState,
            statusCode: clientError.StatusCode
        ).ToActionResult();
    }
}
