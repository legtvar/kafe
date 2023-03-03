using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe.Data;

public record StorageOptions
{
    [Required]
    public string SecretsDirectory { get; init; } = null!;

    [Required]
    public string VideoShardsDirectory { get; init; } = null!;

    public string? ImageShardsDirectory { get; init; }

    public string? SubtitlesShardsDirectory { get; init; }

    public string? GetShardDirectory(ShardKind kind)
    {
        // TODO: Make this more general. It should not depend on the current values in ShardKind.
        return kind switch
        {
            ShardKind.Video => VideoShardsDirectory,
            ShardKind.Image => ImageShardsDirectory,
            ShardKind.Subtitles => SubtitlesShardsDirectory,
            _ => throw new NotSupportedException($"ShardKind '{kind}' is not supported.")
        };
    }
}
