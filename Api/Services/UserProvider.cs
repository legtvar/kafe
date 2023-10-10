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
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class UserProvider
{
    private readonly IHttpContextAccessor contextAccessor;
    private readonly AccountService accountService;
    private readonly IQuerySession query;
    private readonly ILogger<UserProvider> logger;

    public UserProvider(
        IHttpContextAccessor contextAccessor,
        AccountService accountService,
        ILogger<UserProvider> logger,
        IQuerySession query)
    {
        this.contextAccessor = contextAccessor;
        this.accountService = accountService;
        this.logger = logger;
        this.query = query;
    }

    public ApiUser? User { get; private set; }

    public AccountInfo? Account { get; private set; }

    public bool HasExplicitPermission(Hrib entityId, Permission permission)
    {
        return Account is not null && (Account.Permissions.GetValueOrDefault(entityId) & permission) == permission;
    }

    public async Task<bool> HasPermission(
        IVisibleEntity entity,
        Permission permission,
        CancellationToken token = default)
    {
        if (permission == Permission.Read && entity.Visibility == Visibility.Public)
        {
            return true;
        }

        if (permission == Permission.Read && entity.Visibility == Visibility.Internal && Account is not null)
        {
            return true;
        }

        if (HasExplicitPermission(entity.Id, permission))
        {
            return true;
        }

        if (entity is IHierarchicalEntity hierarchicalEntity)
        {
            var cascade = await GetCascadingPermission(hierarchicalEntity, token);
            if ((cascade & permission) == permission)
            {
                return true;
            }
        }
        
        return false;
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

    private async Task<Permission> GetCascadingPermission(IHierarchicalEntity entity, CancellationToken token)
    {
        if (Account is null)
        {
            return Permission.None;
        }

        var mask = Permission.None;
        IEntity? current = entity;
        while (current is IHierarchicalEntity hierarchicalCurrent)
        {
            mask |= Account.Permissions.GetValueOrDefault(current.Id);

            var parentState = await query.Events.FetchStreamStateAsync(hierarchicalCurrent.ParentId);
            if (parentState?.AggregateType is null)
            {
                return mask;
            }

            current = (await query.QueryAsync(
                parentState.AggregateType,
                "where data ->> Id = ?",
                token,
                parentState.Id))
                    .Cast<IEntity>()
                    .FirstOrDefault();
        }

        return mask;
    }
}
