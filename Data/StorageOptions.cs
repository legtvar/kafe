using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data;

public record StorageOptions
{
    public string? ArtifactDirectory { get; init; }
}
