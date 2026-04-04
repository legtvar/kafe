using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kafe.Api;

public class KafeProblemDetailsClientErrorFactory(
    KafeProblemDetailsFactory pdFactory
) : IClientErrorFactory
{
    public IActionResult? GetClientError(ActionContext actionContext, IClientErrorActionResult clientError)
    {
        // TODO: pass clientError.StatusCode
        return pdFactory.CreateProblemDetails(pdFactory.CreateDiagnostic(actionContext.ModelState)).ToActionResult();
    }
}
