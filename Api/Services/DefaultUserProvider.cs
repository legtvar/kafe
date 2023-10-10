using Kafe.Data;
using Kafe.Data.Aggregates;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultUserProvider : IUserProvider
{
    private readonly IHttpContextAccessor contextAccessor;
    private readonly IDocumentSession db;
    private readonly ILogger<DefaultUserProvider> logger;

    public DefaultUserProvider(
        IHttpContextAccessor contextAccessor,
        IDocumentSession db,
        ILogger<DefaultUserProvider> logger)
    {
        if (contextAccessor.HttpContext is null)
        {
            User = ApiUser.System;
            logger.LogWarning($"A scope without a request has been created. " +
                $"Ignore this message if this is intended (e.g. a daemon did it).");
        }
        else
        {
            User = contextAccessor.HttpContext.User.Identities.Any()
                ? ApiUser.FromPrincipal(contextAccessor.HttpContext.User)
                : null;
        }

        this.contextAccessor = contextAccessor;
        this.db = db;
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
        if (User is null)
        {
            throw new InvalidOperationException("Cannot refresh the currrent account because nobody is logged in.");
        }

        if (User == ApiUser.System)
        {
            throw new InvalidOperationException("The system user cannot be refreshed.");
        }

        var account = await db.Events.AggregateStreamAsync<AccountInfo>(User.Id, token: token);
        if (account is null)
        {
            throw new IndexOutOfRangeException("Cannot refresh the current account because it no longer exists.");
        }

        User = ApiUser.FromAggregate(account);

        if (shouldSignIn && contextAccessor.HttpContext is not null)
        {
            // TODO: Pass the current authentication scheme.
            await contextAccessor.HttpContext
                .SignInAsync(User.ToPrincipal(CookieAuthenticationDefaults.AuthenticationScheme));
        }

        logger.LogInformation("The current user has been refreshed.");
    }
}
