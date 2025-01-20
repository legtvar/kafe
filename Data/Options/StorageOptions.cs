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
    public const string DefaultSchema = "public";

    [Required]
    public string SecretsDirectory { get; set; } = null!;

    [Required]
    public string TempDirectory { get; set; } = null!;

    [Required]
    public string ArchiveDirectory { get; set; } = null!;

    [Required]
    public string GeneratedDirectory { get; set; } = null!;

    public string Schema { get; set; } = DefaultSchema;

    public bool AllowSeedData { get; set; } = true;

    public Dictionary<ShardKind, string> ShardDirectories { get; init; } = new()
    {
        [ShardKind.Image] = "images",
        [ShardKind.Video] = "videos",
        [ShardKind.Subtitles] = "subtitles",
        [ShardKind.Blend] = "blends",
        [ShardKind.Unknown] = "unknown"
    };
}
