using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Api.Options;

public record ApiOptions
{
    [Url, Required]
    public string BaseUrl { get; init; } = null!;

    [Required]
    public string AccountConfirmPath { get; init; } = "/account/token";

    [Required]
    public string AccountConfirmRedirectPath { get; init; } = "/auth";

    public List<string> AllowedOrigins { get; init; } = new();
}
