using Kafe.Api.Transfer;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IAccountService
{
    Task CreateTemporaryAccount(
        TemporaryAccountCreationDto dto,
        CancellationToken token = default);

    Task ConfirmTemporaryAccount(
        TemporaryAccountTokenDto confirmationToken,
        CancellationToken token = default);

    Task<AccountDetailDto?> Load(
        Hrib id,
        CancellationToken token = default);

    Task<AccountDetailDto?> Load(
        string emailAddress,
        CancellationToken token = default);

    Task<ApiAccount?> LoadApiAccount(
        Hrib id,
        CancellationToken token = default);

    string EncodeToken(TemporaryAccountTokenDto token);

    bool TryDecodeToken(string encodedToken, [NotNullWhen(true)] out TemporaryAccountTokenDto? dto);
}
