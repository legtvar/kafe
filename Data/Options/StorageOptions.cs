using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data.Options;

public record StorageOptions
{
    [Required]
    public string SecretsDirectory { get; init; } = null!;

    [Required]
    public string TempDirectory { get; init; } = null!;

    [Required]
    public Dictionary<ShardKind, string> ShardDirectories { get; init; } = new();
}
