using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Kafe;

public record struct ShardAnalysis
{
    public static readonly ShardAnalysis Invalid = new()
    {
        IsSuccessful = false
    };
    
    public ShardAnalysis(object shardMetadata, string? fileExtension)
    {
        IsSuccessful = true;
        ShardMetadata = shardMetadata;
        FileExtension = fileExtension;
    }

    [MemberNotNullWhen(true, nameof(ShardMetadata))]
    public bool IsSuccessful { get; init; }

    public string? FileExtension { get; init; }

    public object? ShardMetadata { get; init; }
}
