using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kafe.Api.Options;

public record ApiOptions
{
    public const string DefaultAccountConfirmPath = "/account/token";
    public const string DefaultAccountConfirmRedirectPath = "/auth";

    [Url, Required]
    public string BaseUrl { get; set; } = null!;

    [Url]
    public string FrontEndUrl { get; set; } = null!;

    public string AccountConfirmPath { get; set; } = DefaultAccountConfirmPath;

    [Required]
    public string AccountConfirmRedirectPath { get; init; } = DefaultAccountConfirmRedirectPath;

    public List<string> AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Returns a URL path fragment if the API uses the same hostname as the front end, or absolute URI otherwise.
    /// </summary>
    public string GetFrontEndRedirect(string location)
    {
        return BaseUrl == FrontEndUrl || string.IsNullOrEmpty(FrontEndUrl)
            ? location
            : new Uri(new Uri(FrontEndUrl), location).ToString();
    }
}
