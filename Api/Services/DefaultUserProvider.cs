using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Kafe.Api.Services
{
    public class DefaultUserProvider : IUserProvider
    {
        public DefaultUserProvider(IHttpContextAccessor contextAccessor)
        {
            User = contextAccessor.HttpContext?.User.Identities.Any() == true
                ? ApiUser.FromPrincipal(contextAccessor.HttpContext.User)
                : null;
        }

        public ApiUser? User { get; }
    }
}
