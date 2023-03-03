using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Common;

public record KafeOptions
{
    [Url, Required]
    public string BaseUrl { get; init; } = null!;

    public string? DebugAccountToken { get; init; }
}
