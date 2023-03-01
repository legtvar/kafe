using Kafe.Api.Transfer;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultAccountService : IAccountService
{
    public Task<TemporaryAccountInfoDto> ConfirmTemporaryAccount(
        string confirmationToken,
        CancellationToken token = default)
    {
        throw new System.NotImplementedException();
    }

    public Task CreateTemporaryAccount(
        TemporaryAccountCreationDto dto,
        CancellationToken token = default)
    {
        throw new System.NotImplementedException();
    }
}
