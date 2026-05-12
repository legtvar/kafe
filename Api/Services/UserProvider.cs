using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Services;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Kafe.Api.Services;

public class UserProvider(
    IHttpContextAccessor contextAccessor,
    EntityService entityService,
    AccountService accountService,
    ILogger<UserProvider> logger
)
{
    public const string ImpersonatesClaimType = "impersonates";

    public AccountInfo? Account { get; private set; }

    /// <summary>
    /// Returns the current user's account HRIB or <see cref="Hrib.Empty"/> if the user is anonymous.
    /// </summary>
    public Hrib AccountId => Account?.Id ?? Hrib.Empty;

    public bool HasExplicitPermission(Hrib entityId, Permission permission)
    {
        return Account is not null
            && (Account.Permissions?.GetValueOrDefault(entityId.ToString()) & permission) == permission;
    }

    public async Task<bool> HasPermission(
        Hrib entityId,
        Permission permission,
        CancellationToken token = default
    )
    {
        var effectivePermission = await entityService.GetPermission(
            entityId,
            AccountId,
            token
        );
        return (effectivePermission & permission) == permission;
    }

    public Task<bool> HasPermission(
        IEntity entity,
        Permission permission,
        CancellationToken token = default
    )
    {
        return HasPermission(entity.Id, permission, token);
    }

    public Hrib? GetImpersonatedAccount()
    {
        var user = contextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

        var impersonatedAccountId = user.FindFirst(ImpersonatesClaimType)?.Value;
        if (string.IsNullOrEmpty(impersonatedAccountId))
        {
            return null;
        }

        return impersonatedAccountId;
    }

    public async Task RefreshAccount(ClaimsPrincipal? user = null, CancellationToken token = default)
    {
        user ??= contextAccessor.HttpContext?.User;
        if (user is null)
        {
            Account = null;
            return;
        }

        var id = user.FindFirstValue(ImpersonatesClaimType);
        id ??= user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
        {
            Account = null;
            return;
        }

        var account = (await accountService.Load(id, token: token)).Unwrap();
        logger.LogDebug("Account '{AccountEmailAddress}' ({AccountId}) found.", account.EmailAddress, account.Id);
        Account = account;
    }

    public async Task SignIn(AccountInfo account)
    {
        if (contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("Cannot sign in outside of a request.");
        }

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(Const.AuthenticationCookieExpirationTime),
            IsPersistent = true
        };

        await contextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = GetClaimsPrincipal(
            account: account,
            authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme
        );

        await contextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties
        );

        contextAccessor.HttpContext.User = principal;
        Account = account;
    }

    public async Task Impersonate(AccountInfo? impersonatedAccount)
    {
        if (contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("Cannot impersonate outside of a request.");
        }

        if (Account is null || contextAccessor.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("Cannot impersonate if no user is logged in.");
        }

        var authResult = contextAccessor
            .HttpContext
            .Features
            .GetRequiredFeature<IAuthenticateResultFeature>()
            .AuthenticateResult;
        var authProperties = authResult?.Properties;

        if (authProperties is null)
        {
            throw new InvalidOperationException("Could not obtain current authentication properties.");
        }

        var authScheme = contextAccessor.HttpContext.User.Identity?.AuthenticationType
            ?? authResult?.Ticket?.AuthenticationScheme;

        var principal = GetClaimsPrincipal(
            account: Account,
            authenticationScheme: authScheme,
            existing: contextAccessor.HttpContext.User,
            impersonatedAccountId: impersonatedAccount?.Id
        );

        await contextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties
        );

        contextAccessor.HttpContext.User = principal;
        Account = impersonatedAccount;
    }

    public async Task Unimpersonate()
    {
        if (GetImpersonatedAccount() is null)
        {
            throw new InvalidOperationException("The current account is not impersonating anyone.");
        }

        if (contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("Cannot impersonate outside of a request.");
        }

        if (Account is null || contextAccessor.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("Cannot impersonate if no user is logged in.");
        }

        var authResult = contextAccessor
            .HttpContext
            .Features
            .GetRequiredFeature<IAuthenticateResultFeature>()
            .AuthenticateResult;
        var authProperties = authResult?.Properties;

        if (authProperties is null)
        {
            throw new InvalidOperationException("Could not obtain current authentication properties.");
        }

        var authScheme = contextAccessor.HttpContext.User.Identity?.AuthenticationType
            ?? authResult?.Ticket?.AuthenticationScheme;

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                contextAccessor.HttpContext.User.Claims.Where(c => c.Type != ImpersonatesClaimType),
                authScheme
            )
        );

        await contextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties
        );

        contextAccessor.HttpContext.User = principal;
        await RefreshAccount();
    }

    public static ClaimsPrincipal GetClaimsPrincipal(
        AccountInfo account,
        string? authenticationScheme = null,
        ClaimsPrincipal? existing = null,
        Hrib? impersonatedAccountId = null
    )
    {
        var claims = new List<Claim>();

        if (impersonatedAccountId != null)
        {
            claims.Add(new Claim(ImpersonatesClaimType, impersonatedAccountId.ToString()));
        }

        if (existing is not null)
        {
            foreach (var claim in existing.Claims)
            {
                if (claim.Type == ClaimTypes.NameIdentifier
                    || claim.Type == ClaimTypes.Email
                    || claim.Type == ImpersonatesClaimType
                )
                {
                    continue;
                }

                claims.Add(claim);
            }
        }

        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.Id));
        claims.Add(new Claim(ClaimTypes.Email, account.EmailAddress));

        if (account.PreferredCulture != Const.InvariantCultureCode)
        {
            claims.Add(new Claim(ClaimTypes.StateOrProvince, account.PreferredCulture));
        }

        if (!string.IsNullOrEmpty(account.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, account.Name));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Name, account.EmailAddress));
        }

        if (!string.IsNullOrEmpty(account.Uco))
        {
            claims.Add(new Claim(AccountService.PreferredUsernameClaim, account.Uco));
        }

        var claimsIdentity = new ClaimsIdentity(claims, authenticationScheme);
        return new ClaimsPrincipal(claimsIdentity);
    }
}
