using Kafe.Api.Transfer;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultAccountService : IAccountService
{
    public Task ConfirmTemporaryAccount(string confirmation, CancellationToken token = default)
    {
        throw new System.NotImplementedException();
    }

    public Task CreateTemporaryAccount(TemporaryAccountCreationDto dto, CancellationToken token = default)
    {
        throw new System.NotImplementedException();
    }
}
