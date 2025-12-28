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

    /// <summary>
    /// Whether or not the provided data could be analyzed as the required shard type.
    /// If true, <see cref="ShardMetadata" /> contains appropriate metadata.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ShardMetadata))]
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// File extension that will be used when saving the shard to the file system.
    /// When null, a file extension will be taken from the shard's MIME type using <see cref="FileExtensionMimeMap"/>.
    /// </summary>
    public string? FileExtension { get; init; }

    /// <summary>
    /// An override for the shard's MIME type. This will be sent to the browser upon downloading the shard.
    /// When null, the original MIME type provided by the browser upon upload will be used.
    /// </summary>
    public string? MimeType { get; init; }

    public object? ShardMetadata { get; init; }

    /// <summary>
    /// The shard analyzer that produced this analysis. When null, empty or whitespace, the full name of the shard analyzer type will be
    /// substituted.
    /// </summary>
    public string? ShardAnalyzerName { get; set; }
}
