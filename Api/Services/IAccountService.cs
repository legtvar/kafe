using Kafe.Api.Transfer;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IAccountService
{
    Task CreateTemporaryAccount(TemporaryAccountCreationDto dto, CancellationToken token = default);

    Task ConfirmTemporaryAccount(string confirmationToken, CancellationToken token = default);
}
