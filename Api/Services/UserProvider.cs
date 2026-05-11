using Kafe.Api.Transfer;
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

namespace Kafe.Api.Services;

public class UserProvider(
    IHttpContextAccessor contextAccessor,
    EntityService entityService,
    AccountService accountService,
    ILogger<UserProvider> logger,
    IQuerySession query
)
{
    public const string ImpersonatorClaimType = "impersonator";

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

    public Hrib? GetImpersonator()
    {
        var user = contextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

        return user.FindFirst(ImpersonatorClaimType)?.Value;
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

        logger.LogDebug("Account '{AccountEmailAddress}' ({AccountId}) found.", account.EmailAddress, account.Id);

        Account = account;
    }

    public async Task SignIn(AccountInfo account, bool isImpersonation = false)
    {
        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(Const.AuthenticationCookieExpirationTime),
            IsPersistent = true
        };

        if (contextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException("Cannot sign in outside of a request.");
        }

        await contextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = GetClaimsPrincipal(
            account: account,
            authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme,
            impersonator: contextAccessor.HttpContext.User
        );
        await contextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties
        );

        contextAccessor.HttpContext.User = principal;
        Account = account;
    }

    public static ClaimsPrincipal GetClaimsPrincipal(
        AccountInfo account,
        string? authenticationScheme = null,
        ClaimsPrincipal? existing = null,
        ClaimsPrincipal? impersonator = null
    )
    {
        var claims = new List<Claim>();

        if (impersonator is not null)
        {
            var currentUserId = impersonator.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                claims.Add(new Claim(ImpersonatorClaimType, currentUserId));
            }
        }

        if (existing is not null)
        {
            // Preserve existing claims for non-impersonation sign-ins
            foreach (var claim in existing.Claims)
            {
                if (claim.Type == ClaimTypes.NameIdentifier || claim.Type == ClaimTypes.Email)
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
