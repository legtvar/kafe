using System;
using Kafe.Data;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;

namespace Kafe.Api.Transfer;

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record ShardListDto(
    Hrib Id,
    ShardKind Kind,
    ImmutableArray<string> Variants
);

[SwaggerDiscriminator("kind")]
[SwaggerSubType(typeof(VideoShardDetailDto), DiscriminatorValue = nameof(ShardKind.Video))]
[SwaggerSubType(typeof(ImageShardDetailDto), DiscriminatorValue = nameof(ShardKind.Image))]
[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public abstract record ShardDetailBaseDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId
);

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record VideoShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, MediaDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record ImageShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, ImageDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record SubtitlesShardDetailDto(
    Hrib Id,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, SubtitlesDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record BlendShardDetailDto(
    Hrib Id,
    string? FileName,
    ShardKind Kind,
    Hrib ArtifactId,
    ImmutableDictionary<string, BlendDto> Variants
) : ShardDetailBaseDto(Id, Kind, ArtifactId);

[Obsolete("This type is part of the old artifact abstraction and will soon be replaced.")]
public record ShardCreationDto(
    ShardKind Kind,
    Hrib ArtifactId
);

public record ShardVariantMediaTypeDto(
    Hrib ShardId,
    string Variant,
    string FileExtension,
    string MimeType
);
