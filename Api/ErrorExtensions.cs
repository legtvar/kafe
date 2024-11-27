using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Kafe.Api;

public static class ErrorExtensions
{
    public static ActionResult ToActionResult<T>(this Err<T> err)
    {
        if (!err.HasErrors)
        {
            return new OkObjectResult(err.Value);
        }
        
        return new BadRequestObjectResult(new KafeProblemDetails
        {
            Errors = err.Errors,
            Title = "Bad Request",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Status = 400
        });
    }
}
