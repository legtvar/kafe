using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Api.Transfer;
using Kafe.Data.Capabilities;

namespace Kafe.Api.Services.Account;

public class UserAccountService : IAccountService
{
    private readonly IUserProvider userProvider;
    private readonly SystemAccountService systemService;

    public UserAccountService(IUserProvider userProvider, SystemAccountService systemService)
    {
        this.userProvider = userProvider;
        this.systemService = systemService;
    }

    public Task AddCapabilities(Hrib id, IEnumerable<AccountCapability> capabilities, CancellationToken token = default)
    {
        return ((IAccountService)systemService).AddCapabilities(id, capabilities, token);
    }

    public Task ConfirmTemporaryAccount(TemporaryAccountTokenDto confirmationToken, CancellationToken token = default)
    {
        return ((IAccountService)systemService).ConfirmTemporaryAccount(confirmationToken, token);
    }

    public Task<Hrib> CreateTemporaryAccount(TemporaryAccountCreationDto dto, CancellationToken token = default)
    {
        return ((IAccountService)systemService).CreateTemporaryAccount(dto, token);
    }

    public string EncodeToken(TemporaryAccountTokenDto token)
    {
        return ((IAccountService)systemService).EncodeToken(token);
    }

    public Task<ImmutableArray<AccountListDto>> List(CancellationToken token = default)
    {
        return ((IAccountService)systemService).List(token);
    }

    public Task<AccountDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        return ((IAccountService)systemService).Load(id, token);
    }

    public Task<AccountDetailDto?> Load(string emailAddress, CancellationToken token = default)
    {
        return ((IAccountService)systemService).Load(emailAddress, token);
    }

    public Task<ApiUser?> LoadApiAccount(Hrib id, CancellationToken token = default)
    {
        return ((IAccountService)systemService).LoadApiAccount(id, token);
    }

    public Task<ApiUser?> LoadApiAccount(string emailAddress, CancellationToken token = default)
    {
        return ((IAccountService)systemService).LoadApiAccount(emailAddress, token);
    }

    public bool TryDecodeToken(string encodedToken, [NotNullWhen(true)] out TemporaryAccountTokenDto? dto)
    {
        return ((IAccountService)systemService).TryDecodeToken(encodedToken, out dto);
    }
}
