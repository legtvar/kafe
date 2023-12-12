using Kafe.Common;
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
        
        return new BadRequestObjectResult(err.Errors);
    }
}
