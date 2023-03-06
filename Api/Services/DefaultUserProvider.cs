using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Kafe.Api.Services
{
    public class DefaultUserProvider : IUserProvider
    {
        public DefaultUserProvider(
            IHttpContextAccessor contextAccessor,
            ILogger<DefaultUserProvider> logger)
        {
            if (contextAccessor.HttpContext is null)
            {
                User = ApiUser.System;
                logger.LogWarning($"A scope without a request has been created. " +
                    $"Ignore this message if this is intended (e.g. a daemon did it).");
            }
            else
            {
                User = contextAccessor.HttpContext.User.Identities.Any()
                    ? ApiUser.FromPrincipal(contextAccessor.HttpContext.User)
                    : null;
            }
        }

        public ApiUser? User { get; }
    }
}
