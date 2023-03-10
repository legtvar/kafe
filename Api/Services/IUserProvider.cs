using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IUserProvider
{
    ApiUser? User { get; }

    Task Refresh(bool shouldSignIn = true, CancellationToken token = default);
}
