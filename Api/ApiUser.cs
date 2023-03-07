using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Kafe.Api;

public record ApiUser(
    Hrib Id,
    string? Name,
    string EmailAddress,
    CultureInfo PreferredCulture,
    ImmutableHashSet<AccountCapability> Capabilities)
{
    private static readonly JsonSerializerOptions JsonOptions;

    static ApiUser()
    {
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public static readonly ApiUser System = new(
        Id: Hrib.Invalid,
        Name: Const.SystemName,
        EmailAddress: Const.InvalidEmailAddress,
        PreferredCulture: Const.InvariantCulture,
        Capabilities: ImmutableHashSet.Create<AccountCapability>(new Administration()));

    public static ApiUser FromAggregate(AccountInfo info)
    {
        return new ApiUser(
            Id: info.Id,
            Name: null,
            EmailAddress: info.EmailAddress,
            PreferredCulture: new CultureInfo(info.PreferredCulture),
            Capabilities: info.Capabilities
                .Select(c => (AccountCapability)c)
                .ToImmutableHashSet());
    }

    public static ApiUser? FromPrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identity is null || !principal.Identity.IsAuthenticated)
        {
            return null;
        }

        var capabilityBuilder = ImmutableHashSet.CreateBuilder<AccountCapability>();
        foreach (var roleClaim in principal.FindAll(ClaimTypes.Role))
        {
            var capability = JsonSerializer.Deserialize<AccountCapability>(roleClaim.Value);
            if (capability is null)
            {
                throw new ArgumentException("Failed to deserilize an account capability.");
            }

            capabilityBuilder.Add(capability);
        }

        return new ApiUser(
            Id: principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new ArgumentException("ClaimsPrincipal doesn't contain a NameIdentifier."),
            Name: principal.FindFirstValue(ClaimTypes.Name),
            EmailAddress: principal.FindFirstValue(ClaimTypes.Email)
                ?? throw new ArgumentException("ClaimsPrincipal doesn't contain an Email."),
            PreferredCulture: new CultureInfo(principal.FindFirstValue(ClaimTypes.StateOrProvince)
                ?? Const.InvariantCultureCode),
            Capabilities: capabilityBuilder.ToImmutable()
        );
    }

    public ClaimsPrincipal ToPrincipal(string? authenticationScheme = null)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, Id),
            new Claim(ClaimTypes.Email, EmailAddress)
        };

        if (PreferredCulture.TwoLetterISOLanguageName != Const.InvariantCultureCode)
        {
            claims.Add(new Claim(ClaimTypes.StateOrProvince, PreferredCulture.TwoLetterISOLanguageName));
        }

        if (!string.IsNullOrEmpty(Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, Name));
        }

        foreach (var capability in Capabilities)
        {
            var capabilityString = JsonSerializer.Serialize(capability, JsonOptions);
            claims.Add(new Claim(ClaimTypes.Role, capabilityString));
        }

        var claimsIdentity = new ClaimsIdentity(claims, authenticationScheme);
        return new ClaimsPrincipal(claimsIdentity);
    }
}
