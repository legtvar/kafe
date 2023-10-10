using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data;
using Kafe.Data.Aggregates;

namespace Kafe.Api.Services;

public interface IUserProvider
{
    ApiUser? User { get; }

    AccountInfo? Account { get; }

    Task Refresh(bool shouldSignIn = true, CancellationToken token = default);

    bool HasExplicitPermission(Hrib entityId, Permission permission);

    bool HasPermission(IVisibleEntity entity, Permission permission);
    
    Task RefreshAccount(ClaimsPrincipal? user = null, CancellationToken token = default);
}
