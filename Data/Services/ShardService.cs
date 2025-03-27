using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class ShardService
{
    private readonly IDocumentSession db;
    private readonly StorageService storageService;
    private readonly ShardFactory shardFactory;

    public ShardService(
        IDocumentSession db,
        StorageService storageService,
        ShardFactory shardFactory
    )
    {
        this.db = db;
        this.storageService = storageService;
        this.shardFactory = shardFactory;
    }

    public async Task<ShardInfo?> Load(Hrib id, CancellationToken token = default)
    {
        return await db.LoadAsync<ShardInfo>(id.ToString(), token);
    }

    public async Task<Hrib?> Create(
        KafeType shardType,
        Hrib artifactId,
        Stream stream,
        string mimeType,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        shardId ??= Hrib.Create();
        stream.Seek(0, SeekOrigin.Begin);

        var tmpPath = await storageService.TryStoreTemporaryShard(shardId, stream, token);
        if (tmpPath is null)
        {
            return null;
        }

        await shardFactory.Create(shardType, tmpPath, )

        var shardTypeMetadata = shardTypes.Shards.GetValueOrDefault(shardType)
            ?? throw new ArgumentException($"Shard type '{shardType}' could not be recognized.");
        shardTypeMetadata.
        return kind switch
        {
            ShardKind.Video => await CreateVideo(artifactId, stream, mimeType, shardId, token),
            ShardKind.Image => await CreateImage(artifactId, stream, shardId, token),
            ShardKind.Subtitles => await CreateSubtitles(artifactId, stream, shardId, token),
            ShardKind.Blend => await CreateBlend(artifactId, stream, shardId, token),
            _ => throw new NotSupportedException($"Creation of '{kind}' shards is not supported yet.")
        };
    }

    private async Task<Hrib?> CreateImage(
        Hrib artifactId,
        Stream imageStream,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        var imageInfo = await imageService.GetInfo(imageStream, token);

        imageStream.Seek(0, SeekOrigin.Begin);

        shardId ??= Hrib.Create();

        var created = new ImageShardCreated(
            ShardId: shardId.ToString(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.ToString(),
            OriginalVariantInfo: imageInfo);

        if (!await storageService.TryStoreShard(
            ShardKind.Image,
            created.ShardId,
            imageStream,
            Const.OriginalShardVariant,
            imageInfo.FileExtension,
            token))
        {
            throw new InvalidOperationException("The image could not be stored.");
        }

        db.Events.KafeStartStream<VideoShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return created.ShardId;
    }

    private async Task<Hrib?> CreateBlend(
        Hrib artifactId,
        Stream blendStream,
        Hrib? shardId = null,
        CancellationToken token = default)
    {
        var blendInfo = new BlendInfo(".blend", "application/x-blender");
        blendStream.Seek(0, SeekOrigin.Begin);
        shardId ??= Hrib.Create();

        var created = new BlendShardCreated(
            ShardId: shardId.ToString(),
            CreationMethod: CreationMethod.Api,
            ArtifactId: artifactId.ToString(),
            OriginalVariantInfo: blendInfo);

        if (!await storageService.TryStoreShard(
            ShardKind.Blend,
            created.ShardId,
            blendStream,
            Const.OriginalShardVariant,
            blendInfo.FileExtension,
            token))
        {
            throw new InvalidOperationException("The .blend file could not be stored.");
        }

        db.Events.KafeStartStream<BlendShardInfo>(created.ShardId, created);
        await db.SaveChangesAsync(token);
        return created.ShardId;
    }

    public async Task<Stream> OpenStream(Hrib id, string? variant, CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);
        var shardKind = await GetShardKind(id, token);
        if (!storageService.TryOpenShardStream(shardKind, id, variant, out var stream, out _))
        {
            throw new ArgumentException($"A shard stream for the '{id}' shard could not be opened.");
        }

        return stream;
    }

    public record VariantInfo(
        Hrib ShardId,
        string Variant,
        string? FileExtension,
        string? MimeType
    );

    public async Task<VariantInfo?> GetShardVariantMediaType(
        Hrib id,
        string? variant,
        CancellationToken token = default)
    {
        variant = SanitizeVariantName(variant);

        // TODO: Get rid of this switch.
        var shard = await Load(id, token);
        if (shard is null)
        {
            return null;
        }

        return shard.Kind switch
        {
            ShardKind.Video => ((VideoShardInfo)shard).Variants.TryGetValue(variant, out var media)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: media.FileExtension,
                    MimeType: media.MimeType)
                : null,
            ShardKind.Image => ((ImageShardInfo)shard).Variants.TryGetValue(variant, out var image)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType)
                : null,
            ShardKind.Subtitles => ((SubtitlesShardInfo)shard).Variants.TryGetValue(variant, out var image)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: image.FileExtension,
                    MimeType: image.MimeType)
                : null,
            ShardKind.Blend => ((BlendShardInfo)shard).Variants.TryGetValue(variant, out var media)
                ? new VariantInfo(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: media.FileExtension,
                    MimeType: media.MimeType)
                : null,
            _ => throw new NotImplementedException()
        }; ;
    }

    private static string SanitizeVariantName(string? variant)
    {
        // "original" is always the default
        variant ??= Const.OriginalShardVariant;

        // ignore any file extension
        variant = Path.GetFileNameWithoutExtension(variant);

        return variant;
    }
}
