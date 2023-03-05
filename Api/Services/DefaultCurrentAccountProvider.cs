using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Kafe.Api.Services
{
    public class DefaultCurrentAccountProvider : ICurrentAccountProvider
    {
        public DefaultCurrentAccountProvider(IHttpContextAccessor contextAccessor)
        {
            User = contextAccessor.HttpContext?.User.Identities.Any() == true
                ? ApiAccount.FromPrincipal(contextAccessor.HttpContext.User)
                : null;
        }

        public ApiAccount? User { get; }
    }
}
