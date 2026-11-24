using Kafe.Data;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ShardListDto(
    Hrib Id,
    ShardKind Kind,
    ImmutableArray<string> Variants,
    string? UploadFileName);

[SwaggerDiscriminator("kind")]
[SwaggerSubType(typeof(VideoShardDetailDto), DiscriminatorValue = nameof(ShardKind.Video))]
[SwaggerSubType(typeof(ImageShardDetailDto), DiscriminatorValue = nameof(ShardKind.Image))]
public abstract record ShardDetailBaseDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    string? UploadFileName = null);

public record VideoShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, MediaDto> Variants,
    string? UploadFileName = null
) : ShardDetailBaseDto(Id, Kind, ArtifactId, UploadFileName);

public record ImageShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, ImageDto> Variants,
    string? UploadFileName = null
) : ShardDetailBaseDto(Id, Kind, ArtifactId, UploadFileName);

public record SubtitlesShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, SubtitlesDto> Variants,
    string? UploadFileName = null
) : ShardDetailBaseDto(Id, Kind, ArtifactId, UploadFileName);

public record BlendShardDetailDto(
    Hrib Id,
    string? FileName,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, BlendDto> Variants,
    string? UploadFileName = null
) : ShardDetailBaseDto(Id, Kind, ArtifactId, UploadFileName);

public record BlobShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    BlobDto Info,
    string? UploadFileName = null
) : ShardDetailBaseDto(Id, Kind, ArtifactId, UploadFileName);

public record ShardCreationDto(
    ShardKind Kind,
    Hrib ArtifactId);

public record ShardVariantMediaTypeDto(
    Hrib ShardId,
    string Variant,
    string FileExtension,
    string MimeType
);
