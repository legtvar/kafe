using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Kafe.Data.Options;
using Kafe.Data.Services;
using Kafe.Media;
using Marten;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultShardService : IShardService
{
    private readonly IDocumentSession db;
    private readonly IStorageService storageService;
    private readonly IMediaService mediaService;

    public DefaultShardService(
        IDocumentSession db,
        IStorageService storageService,
        IMediaService mediaService)
    {
        this.db = db;
        this.storageService = storageService;
        this.mediaService = mediaService;
    }

    public async Task<ShardDetailBaseDto?> Load(Hrib id, CancellationToken token = default)
    {
        var shardKind = await GetShardKind(id, token);
        if (shardKind == ShardKind.Unknown)
        {
            return null;
        }

        ShardInfoBase? shard = shardKind switch
        {
            ShardKind.Video => await db.LoadAsync<VideoShardInfo>(id, token),
            ShardKind.Image => await db.LoadAsync<ImageShardInfo>(id, token),
            ShardKind.Subtitles => await db.LoadAsync<SubtitlesShardInfo>(id, token),
            _ => throw new NotSupportedException($"ShardKind '{shardKind}' is not supported.")
        };

        if (shard is null)
        {
            return null;
        }

        return TransferMaps.ToShardDetailDto(shard);
    }

    public async Task<Hrib?> Create(
        ShardCreationDto dto,
        string mimeType,
        Stream stream,
        CancellationToken token = default)
    {
        var artifact = await db.LoadAsync<ArtifactInfo>(dto.ArtifactId, token);
        if (artifact is null)
        {
            throw new ArgumentException($"Artifact '{dto.ArtifactId}' does not exist.");
        }

        return dto.Kind switch
        {
            ShardKind.Video => await CreateVideo(dto, mimeType, stream, token),
            _ => throw new NotSupportedException($"Creation of '{dto.Kind}' shards is not supported yet.")
        };
    }

    private async Task<Hrib?> CreateVideo(
        ShardCreationDto dto,
        string mimeType,
        Stream videoStream,
        CancellationToken token = default)
    {
        if (mimeType != Const.MatroskaMimeType && mimeType != Const.Mp4MimeType)
        {
            throw new ArgumentException($"Only '{Const.MatroskaMimeType}' and '{Const.Mp4MimeType}' video container " +
                $"formats are supported.");
        }

        var shardId = Hrib.Create();
        var originalFileExtension = mimeType == Const.MatroskaMimeType
            ? Const.MatroskaFileExtension
            : Const.Mp4FileExtension;
        await storageService.TryStoreShard(
            dto.Kind,
            shardId,
            videoStream,
            Const.OriginalShardVariant,
            originalFileExtension,
            token);

        if (!storageService.TryOpenShardStream(
            dto.Kind,
            shardId,
            Const.OriginalShardVariant,
            out var shardStream,
            out var shardFileExtension))
        {
            throw new ArgumentException("The shard stream could not be opened just after being saved.");
        }

        var mediaInfo = await mediaService.GetInfo(shardStream, token);
        mediaInfo = mediaInfo with { FileExtension = shardFileExtension };

        var created = new VideoShardCreated(
            ShardId: shardId,
            CreationMethod: CreationMethod.Api,
            ArtifactId: dto.ArtifactId,
            OriginalVariantInfo: mediaInfo);
        db.Events.StartStream<VideoShardInfo>(created.ShardId, created);

        await db.SaveChangesAsync(token);
        return shardId;
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

    public async Task<ShardKind> GetShardKind(Hrib id, CancellationToken token = default)
    {
        var firstEvent = (await db.Events.FetchStreamAsync(id, 1, token: token)).SingleOrDefault();
        if (firstEvent is null)
        {
            return ShardKind.Unknown;
        }

        return ((IShardCreated)firstEvent.Data).GetShardKind();
    }

    public async Task<ShardVariantMediaTypeDto?> GetShardVariantMediaType(
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
            ShardKind.Video => ((VideoShardDetailDto)shard).Variants.TryGetValue(variant, out var media)
                ? new ShardVariantMediaTypeDto(
                    ShardId: shard.Id,
                    Variant: variant,
                    FileExtension: media.FileExtension,
                    MimeType: media.MimeType)
                : null,
            _ => throw new NotImplementedException()
        };
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
