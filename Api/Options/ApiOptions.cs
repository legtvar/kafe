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

    public string AccountConfirmPath { get; set; } = DefaultAccountConfirmPath;

    [Required]
    public string AccountConfirmRedirectPath { get; init; } = DefaultAccountConfirmRedirectPath;

    public List<string> AllowedOrigins { get; init; } = new();
}
