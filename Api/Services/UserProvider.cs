using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor contextAccessor;
    private readonly AccountService accountService;
    private readonly ILogger<UserProvider> logger;

    public UserProvider(
        IHttpContextAccessor contextAccessor,
        AccountService accountService,
        ILogger<UserProvider> logger)
    {
        this.contextAccessor = contextAccessor;
        this.accountService = accountService;
        this.logger = logger;
    }

    public ApiUser? User { get; private set; }

    public AccountInfo? Account { get; private set; }

    public bool HasExplicitPermission(Hrib entityId, Permission permission)
    {
        return Account is not null && Account.Permissions.GetValueOrDefault(entityId) >= permission;
    }

    public bool HasPermission(IVisibleEntity entity, Permission permission)
    {
        if (permission == Permission.Read && entity.Visibility == Visibility.Public)
        {
            return true;
        }

        if (permission == Permission.Read && entity.Visibility == Visibility.Internal && Account is not null)
        {
            return true;
        }

        return HasExplicitPermission(entity.Id, permission);
    }

    public async Task Refresh(bool shouldSignIn = true, CancellationToken token = default)
    {
        throw new NotSupportedException();
    }

    public async Task RefreshAccount(ClaimsPrincipal? user = null, CancellationToken token = default)
    {
        user ??= contextAccessor.HttpContext?.User;
        if (user is null)
        {
            Account = null;
            return;
        }

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
        {
            Account = null;
            return;
        }

        var account = await accountService.Load(id, token: token);
        if (account is null)
        {
            throw new IndexOutOfRangeException($"Account with id '{id}' does not exist.");
        }

        logger.LogDebug("Account '{}' ({}) found.", account.EmailAddress, account.Id);

        Account = account;
    }

    public async Task SignIn(AccountInfo account)
    {
        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(Const.AuthenticationCookieExpirationTime),
            IsPersistent = true
            //RedirectUri = options.Value.AccountConfirmRedirectPath
        };

        if (contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("Cannot sign in outside of a request.");
        }

        await contextAccessor.HttpContext.SignInAsync(GetClaimsPrincipal(account), authProperties);
        Account = account;
    }

    public static ClaimsPrincipal GetClaimsPrincipal(AccountInfo account, string? authenticationScheme = null)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id),
            new Claim(ClaimTypes.Email, account.EmailAddress)
        };

        if (account.PreferredCulture != Const.InvariantCultureCode)
        {
            claims.Add(new Claim(ClaimTypes.StateOrProvince, account.PreferredCulture));
        }

        // TODO: Let accounts have Name
        // if (!string.IsNullOrEmpty(account.Name))
        // {
        //     claims.Add(new Claim(ClaimTypes.Name, Name));
        // }
        claims.Add(new Claim(ClaimTypes.Name, account.EmailAddress));

        var claimsIdentity = new ClaimsIdentity(claims, authenticationScheme);
        return new ClaimsPrincipal(claimsIdentity);
    }
}
