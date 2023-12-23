using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Kafe.MockIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            { };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
                new Client
            {
                ClientName = "KAFE dev",
                ClientId = "kafe",
                ClientSecrets =
                {
                    new Secret("coffein".Sha512())
                },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris =
                {
                    "https://localhost:44369/signin-oidc"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                },
                AlwaysIncludeUserClaimsInIdToken = true
            }
        };
}
