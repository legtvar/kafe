using Kafe.Api.Transfer;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IAccountService
{
    Task CreateTemporaryAccount(
        TemporaryAccountCreationDto dto,
        CancellationToken token = default);

    Task<TemporaryAccountInfoDto?> ConfirmTemporaryAccount(
        string confirmationToken,
        CancellationToken token = default);

    Task<AccountDetailDto?> Load(
        Hrib id,
        CancellationToken token = default);
}
