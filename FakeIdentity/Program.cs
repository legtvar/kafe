var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenIddict(b =>
    b.AddServer(o => {
        o.SetAuthorizationEndpointUris("/authorize");
        o.SetTokenEndpointUris("/token");
        o.AllowAuthorizationCodeFlow();
        o.AddDevelopmentEncryptionCertificate();
        o.AddDevelopmentSigningCertificate();
        o.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough();
    }));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapGet("/authorize", async (HttpContext context) =>
{
    // Resolve the claims stored in the principal created after the Steam authentication dance.
    // If the principal cannot be found, trigger a new challenge to redirect the user to Steam.
    var principal = (await context.AuthenticateAsync(SteamAuthenticationDefaults.AuthenticationScheme))?.Principal;
    if (principal is null)
    {
        return Results.Challenge(properties: null, new[] { SteamAuthenticationDefaults.AuthenticationScheme });
    }

    var identifier = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    // Create a new identity and import a few select claims from the Steam principal.
    var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
    identity.AddClaim(new Claim(Claims.Subject, identifier));
    identity.AddClaim(new Claim(Claims.Name, identifier).SetDestinations(Destinations.AccessToken));

    return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

app.Run();
