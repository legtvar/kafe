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

public class UserProvider
{
    private readonly IHttpContextAccessor contextAccessor;
    private readonly EntityService entityService;
    private readonly AccountService accountService;
    private readonly IQuerySession query;
    private readonly ILogger<UserProvider> logger;

    public UserProvider(
        IHttpContextAccessor contextAccessor,
        EntityService entityService,
        AccountService accountService,
        ILogger<UserProvider> logger,
        IQuerySession query)
    {
        this.contextAccessor = contextAccessor;
        this.entityService = entityService;
        this.accountService = accountService;
        this.logger = logger;
        this.query = query;
    }

    public AccountInfo? Account { get; private set; }

    public bool HasExplicitPermission(Hrib entityId, Permission permission)
    {
        return Account is not null && (Account.Permissions?.GetValueOrDefault(entityId) & permission) == permission;
    }

    public async Task<bool> HasPermission(
        Hrib entityId,
        Permission permission,
        CancellationToken token = default)
    {
        var result = await query.QueryAsync<bool>(
            "SELECT kafe_get_resource_perms(?, ?) & ? = ?",
            token,
            entityId.Value,
            Account?.Id!,
            (int)permission,
            (int)permission);
        return result?.Single() ?? false;
    }

    public Task<bool> HasPermission(
        IEntity entity,
        Permission permission,
        CancellationToken token = default)
    {
        return HasPermission(entity.Id, permission, token);
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

        await contextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            GetClaimsPrincipal(account, CookieAuthenticationDefaults.AuthenticationScheme),
            authProperties);
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
