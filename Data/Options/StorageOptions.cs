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
    public string ArchiveDirectory { get; init; } = null!;

    [Required]
    public string GeneratedDirectory { get; init; } = null!;

    public Dictionary<ShardKind, string> ShardDirectories { get; init; } = new()
    {
        [ShardKind.Image] = "images",
        [ShardKind.Video] = "videos",
        [ShardKind.Subtitles] = "subtitles",
        [ShardKind.Unknown] = "unknown"
    };
}
