using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Json;

namespace Kafe.Api;

public record ApiUser(
    Hrib Id,
    string? Name,
    string EmailAddress,
    string PreferredCulture,
    ImmutableHashSet<IAccountCapability> Capabilities)
{
    private static readonly JsonSerializerOptions JsonOptions;

    static ApiUser()
    {
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public static ApiUser FromAggregate(AccountInfo info)
    {
        return new ApiUser(
            Id: info.Id,
            Name: null,
            EmailAddress: info.EmailAddress,
            PreferredCulture: info.PreferredCulture,
            Capabilities: info.Capabilities);
    }

    public static ApiUser FromPrincipal(ClaimsPrincipal principal)
    {
        var capabilityBuilder = ImmutableHashSet.CreateBuilder<IAccountCapability>();
        foreach (var roleClaim in principal.FindAll(ClaimTypes.Role))
        {
            var capability = JsonSerializer.Deserialize<IAccountCapability>(roleClaim.Value);
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
            PreferredCulture: principal.FindFirstValue(ClaimTypes.StateOrProvince) ?? Const.InvariantCultureCode,
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

        if (PreferredCulture != Const.InvariantCultureCode)
        {
            claims.Add(new Claim(ClaimTypes.StateOrProvince, PreferredCulture));
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
