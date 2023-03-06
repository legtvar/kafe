using Kafe.Api.Transfer;
using Kafe.Data.Capabilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IAccountService
{
    Task<Hrib> CreateTemporaryAccount(
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

    Task<ApiUser?> LoadApiAccount(
        Hrib id,
        CancellationToken token = default);

    Task<ApiUser?> LoadApiAccount(
        string emailAddress,
        CancellationToken token = default);

    Task AddCapabilities(
        Hrib id,
        IEnumerable<AccountCapability> capabilities,
        CancellationToken token = default);

    string EncodeToken(TemporaryAccountTokenDto token);

    bool TryDecodeToken(string encodedToken, [NotNullWhen(true)] out TemporaryAccountTokenDto? dto);
}
