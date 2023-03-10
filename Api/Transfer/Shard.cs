using Kafe.Data;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

public record ShardListDto(
    Hrib Id,
    ShardKind Kind,
    ImmutableArray<string> Variants);

[SwaggerDiscriminator("kind")]
[SwaggerSubType(typeof(VideoShardDetailDto), DiscriminatorValue = nameof(ShardKind.Video))]
[SwaggerSubType(typeof(ImageShardDetailDto), DiscriminatorValue = nameof(ShardKind.Image))]
public abstract record ShardDetailBaseDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId);

public record VideoShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, MediaDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

public record ImageShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, ImageDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

public record SubtitlesShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, SubtitlesDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

public record ShardCreationDto(
    ShardKind Kind,
    Hrib ArtifactId);

public record ShardVariantMediaTypeDto(
    Hrib ShardId,
    string Variant,
    string FileExtension,
    string MimeType
);
